# Mokkit unit-test conventions (a different stack)

This project is the **counterpart** to the integration suite, and its main job is to prove Mokkit is
**not bound to any test framework or mocking library**. Same Mokkit Arrange/Act/Inspect flow; a
completely different surrounding stack.

| Concern | Integration suite | This unit suite |
|---|---|---|
| Test framework | NUnit | **xUnit** |
| Mocking | Moq (`Mokkit.Containers.Moq`) | **NSubstitute** (custom container, see below) |
| Assertions | NUnit `Assert` + Verify snapshots | **Shouldly** |
| Test data | hand-written defaults | **Bogus** (`ClientFaker`, fixed seed) |
| Infrastructure | real Postgres (Respawn) | **none** — pure unit, mock the one outward dep |

The Mokkit pieces (`Stage`, `Arrange`, `Inspect`, `Capture`, `host.Execute`) are identical to the
integration suite — only the libraries around them changed.

---

## 1. Bring-your-own mock library: the custom NSubstitute container

Mokkit ships Moq, NSubstitute and FakeItEasy container adapters out of the box. This suite deliberately
**rolls its own** NSubstitute container instead — to demonstrate that the adapter contract is tiny, so you
can plug in *any* mocking (or DI) library by implementing `IDependencyContainerBuilder` — see [Containers/](Containers/):

- `SubstituteContainerBuilder` — the builder (`UseInit` registers substitutes).
- `SubstituteCollection` / `SubstituteRegistration` — `AddSubstitute<T>()` records `typeof(T)` and a
  `() => Substitute.For<T>()` factory.
- `SubstituteContainer` + `SubstituteScope` — per test scope, creates one fake per registration and, in
  `OnAsyncScopeEnter()`, publishes each into the `TestHostBag` **keyed by the service interface**
  (`bag.TryAdd(reg.InnerType, substitute)`). That single line is the whole contract; the existing
  `ResolveFromStage` glue then injects the fake into real services.

Because an NSubstitute fake **is** the interface (no `.Object` wrapper like Moq), the substitute, the
injected dependency, and the test-side handle are the **same object**. So you configure and verify the
interface directly:

```csharp
// arrange
host.Execute<IDistributedCache>(c => c.GetAsync(key, Arg.Any<CancellationToken>()).Returns(bytes));
// inspect
host.Execute<IDistributedCache>(c => c.Received(1).SetAsync(key, ...));
```

---

## 2. Harness: one stage composition per system-under-test

Mokkit builds its containers **once** (`TestStageSetup.Create`); `EnterStage()` only spins a fresh stage
over that fixed composition. A type can't be both the real SUT and a substitute across different tests
(`IClientStatusChangedProcessor` is real for the processor test but a fake for the consumer test;
`IKafkaEventPublisher` is real for the publisher but a fake for the processor). So **each SUT gets its own
fixture**:

- [`BaseStageFixture`](BaseStageFixture.cs) (`IAsyncLifetime`) — common plumbing (substitute container +
  DI container with `NullLogger`, substitutes bridged via `ResolveFromStage`), with abstract
  `ConfigureSubstitutes` / `ConfigureServices`.
- One small fixture per SUT, **colocated with that SUT's tests/arranges/inspects**:
  `Cache/CacheServiceFixture`, `Validation/ValidatorFixture`, `Messaging/Processor/ProcessorFixture`,
  `Messaging/Publisher/PublisherFixture`, `Messaging/Consumer/ConsumerFixture` — each registers the
  **real SUT** and the **substitutes for that SUT's direct dependencies**.
- [`BaseUnitTest<TFixture>`](BaseUnitTest.cs) uses `IClassFixture<TFixture>`, enters a fresh stage per
  test, exposes `Stage`/`Arrange`/`Inspect`. Test classes derive from `BaseUnitTest<TheFixture>`.

### Layout — one folder per concern (SUT)

```
BaseStageFixture.cs · BaseUnitTest.cs · Containers/        # shared plumbing + the NSubstitute container
Cache/        CacheServiceFixture · ArrangeCache · InspectCache · ClientFaker · ClientCacheServiceTests
Validation/   ValidatorFixture · ArrangeCommand · InspectValidation · SaveClientCommandValidatorTests
Messaging/    KafkaMessageFaker                              # shared messaging test-data
  Processor/  ProcessorFixture · ArrangeProcessor · InspectProcessor · ClientStatusChangedProcessorTests
  Publisher/  PublisherFixture · ArrangeProducer · InspectProducer · KafkaEventPublisherTests
  Consumer/   ConsumerFixture · ArrangeConsumer · InspectConsumer · ClientEventConsumerTests
```

Each concern folder is its own namespace, so its arranges/inspects are in scope for its tests without
extra `using`s. Messaging is one concern with several SUTs, so it gets a subfolder per SUT and keeps only
genuinely shared items (the message faker) at its root — there are no cross-SUT arranges/inspects to share.

---

## 3. Everything is arrange, everything is inspect

The cardinal rule (matching the integration suite): **no raw setup or assertions in the test body.** Every
piece of data and every prepared mock is a business-named `Arrange`; every check is a business-named
`Inspect`. Captures (`Capture<T>`) carry shared data (a built client, command, message, or `ConsumeResult`)
from arrange to inspect.

```csharp
await Arrange.MessageAvailable(out var message);   // builds ConsumeResult + sets up IConsumer
var processed = await ConsumeOnce();               // ACT
await Inspect
    .MessageConsumed(processed)                     // a message was handled
    .ForwardedToProcessor(message)                  // processor.ProcessAsync(value) called
    .OffsetCommitted(message);                      // consumer.Commit(result) called
```

**Two Act styles — the dependencies always come from the stage either way.** (Act is a first-class Mokkit
phase, used by the integration and E2E suites as `Act.Verb(...)` vocabulary; here every act is a trivial
one-liner, so this suite keeps them as inline `Stage.Execute` helpers rather than a separate `Act` file.)
- *Resolve the SUT from the stage* (Cache, Validator, Processor): the real SUT is registered in the
  fixture and `Act` resolves it, so Mokkit constructs it with the substitutes injected —
  `Stage.ExecuteAsync<IClientCacheService, Client?>(svc => svc.GetClientAsync(id))`.
- *Construct the SUT in `Act`* (Publisher, Consumer): `Act` news up the SUT but pulls each dependency from
  the stage via the multi-service `Stage.Execute` overloads —
  `Stage.ExecuteAsync<IProducer<…>, ILogger<…>>((p, log) => new KafkaEventPublisher(p, log).Publish…)`.

(Cache note: `GetStringAsync`/`SetStringAsync` are extension methods NSubstitute can't intercept, so the
arrange/inspect helpers configure/verify the real members `GetAsync`/`SetAsync` they delegate to.)

**Act returns an artifact; Inspect only observes.** Every `Act` here hands the inspects a handle to what it
did — a result value (`Client?`, `bool processed`), a captured `Exception?`, or an effect observable on the
substitute (`Received`). Inspects then only *observe*; none of them performs the action under test. This is
the same rule the E2E suite documents in full ([E2E-CONVENTIONS.md](../Mokkit.Example1.E2E.Tests/E2E-CONVENTIONS.md) §3).

---

## 4. What's tested (no database)

- [`ClientCacheServiceTests`](Cache/ClientCacheServiceTests.cs) — the real `ClientCacheService` with a
  substituted `IDistributedCache`: cache hit/miss/throw on read, serialize-and-store with the 30-minute
  expiry, and remove.
- [`SaveClientCommandValidatorTests`](Validation/SaveClientCommandValidatorTests.cs) — the validator
  resolved from the stage; `[Theory]`-driven rule coverage with Shouldly.
- [`Messaging/`](Messaging/) — the Kafka consume path (see §5):
  - `ClientStatusChangedProcessorTests` — the real processor, resolving its save handler + publisher
    from a real DI scope while Mokkit serves substitutes into that scope.
  - `KafkaEventPublisherTests` / `ClientEventConsumerTests` — fake the actual Confluent
    `IProducer`/`IConsumer`.

Handlers depend on `ExampleContext` (EF), so they belong to the integration suite, not here.

---

## 5. Kafka: faking the lib + testing a class with deep dependencies

The Kafka code was refactored so it is testable: `KafkaEventPublisher`/`ClientEventConsumer` take an
injected Confluent `IProducer`/`IConsumer`, and the consume logic lives in a transport-free
`ClientStatusChangedProcessor` (+ `ConsumeOnceAsync` on the consumer). Three patterns on show:

- **Fake the actual Kafka lib interfaces — through the stage.** `PublisherFixture`/`ConsumerFixture`
  substitute the Confluent `IProducer`/`IConsumer`; the arrange helpers
  ([`ArrangeProducer`](Messaging/Publisher/ArrangeProducer.cs),
  [`ArrangeConsumer`](Messaging/Consumer/ArrangeConsumer.cs)) configure them and the inspects verify them —
  never inline. The SUT is built in `Act` pulling those fakes from the stage.
- **Mokkit serving fakes into a real DI scope.** The processor resolves its handler + publisher via
  `IServiceScopeFactory.CreateScope()`. `ProcessorFixture` substitutes `IRequestHandler<SaveClientCommand,…>`
  and `IKafkaEventPublisher`; Mokkit's `ResolveFromStage` serves them into the processor's child scope, and
  the arrange/inspect helpers configure/verify them through the stage — same instances the processor uses.
- **Faking internal interfaces.** `IClientStatusChangedProcessor` is `internal`; Castle DynamicProxy
  builds proxies in a dynamic assembly, so Infrastructure exposes internals with
  `[InternalsVisibleTo("DynamicProxyGenAssembly2")]` (alongside the test assembly) to allow substituting it.

---

## 6. Running

```bash
dotnet test src/Mokkit.Example1.Unit.Tests   # no Docker / Postgres required
```
