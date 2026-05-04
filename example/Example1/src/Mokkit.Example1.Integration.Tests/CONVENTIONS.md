# Mokkit integration-test conventions

This project is a worked example of writing integration tests with **Mokkit** in an
Arrange–Act–Inspect (AAA) style. Tests read like specifications: the *what* lives in the test
body, the *how* lives in reusable arrange/inspect building blocks. There is no DSL or codegen —
everything here is plain C# extension methods over Mokkit's fluent interfaces.

The client domain (`GetClient`, `SaveClient`) is the system under test. Read this document before
adding tests, then copy the nearest existing feature as a template.

---

## 1. The shape of a test

Every test is three blocks, in order, with nothing else interleaved:

```csharp
[Test]
public async Task Create_PersistsClient_UpdatesCache_AndPublishesCreatedEvent()
{
    // ARRANGE — fluent chain of domain-named extension methods on ITestArrange
    await Arrange
        .Clock(ArrangeClient.FixedUtcNow)
        .Ids(ArrangeClient.FixedClientId)
        .CreateClientCommand(out var command, WithName("Acme Corporation"));

    // ACT — a private helper resolves the handler from the stage and calls it
    var result = await Act(command);

    // INSPECT — fluent chain on ITestInspect / ITestInspectScope<T>
    await Inspect
        .SaveResult(result).IsSuccess(ArrangeClient.FixedClientId)
        .DbClientById(ArrangeClient.FixedClientId, out var saved, c => Assert.That(c, Is.Not.Null))
        .Verify(saved)
        .CacheUpdated(ArrangeClient.FixedClientId)
        .EventPublished(ArrangeClient.FixedClientId, "created");
}
```

`Arrange` and `Inspect` are properties on [`BaseIntegrationTest`](BaseIntegrationTest.cs); each
starts a fresh chain on the current test stage. `await` runs the chain.

---

## 2. What is real and what is mocked

Configured once in [`BaseIntegrationTest`](BaseIntegrationTest.cs):

| Dependency | Strategy | Why |
|---|---|---|
| Postgres (`ExampleContext`) | **Real**, reset by Respawn between tests | the persistence boundary is what we want to prove |
| Application layer (handlers, validators) | **Real** (`AddApplicationLayer`) | the code under test |
| `IClientCacheService` (Redis) | **Mock** | arrange cache hit/miss; verify writes — no container needed |
| `IKafkaEventPublisher` (Kafka) | **Mock** | verify events — no broker needed |
| `IDateTimeProvider`, `IIdGenerator` | **Mock** | make time and ids deterministic and assertable |

The mocks are registered with `MoqContainerBuilder.AddMock<T>()` and bridged into the real DI graph
with `ResolveFromStage(...)`, so the real handler receives the mocked `.Object`.

A fresh database (`example1_test_<guid>`) is created per fixture and dropped at the end; override the
connection string with the `TEST_DATABASE` environment variable.

---

## 3. Arranges

Arrange helpers are **extension methods on `ITestArrange`** that return `ITestArrange` and end in
`.Then(host => …)`. Group them by scope:

- **Cross-feature** — [`Helpers/ArrangeClient.cs`](Helpers/ArrangeClient.cs): `Clock`, `Ids`,
  `DbClient` (seeds the real DB), and shared defaults / `NewDefaultClient`.
- **Per-feature** — e.g. [`ArrangeSaveClient`](Features/Client/SaveClient/ArrangeSaveClient.cs),
  [`ArrangeGetClient`](Features/Client/GetClient/ArrangeGetClient.cs).

Rules:

1. **Capture, don't return.** Surface values built during arrange with an `out Capture<T>` parameter;
   the test passes the capture into the inspect phase.
2. **Set up mocks inside `host.Execute<Mock<T>>(…)`**; seed the DB inside
   `host.ExecuteAsync<ExampleContext>(…)`. Async work must be **awaited** — return the `Task` from
   `.Then(async host => await …)`, never fire-and-forget.
3. **Defaults at the top, variation via mutators.** A command is built from one default and a
   `params ClientMutateFn[]`; tests list only what differs: `CreateClientCommand(out var c,
   WithEmail("bad"))`. Mutators (`WithName`, `WithEmail`, …) are small static factories.
4. **Captures are populated when the chain is awaited.** Do not read `capture.Value` while *building*
   a later arrange in the same chain — it is still null. If arrange step B needs a value produced by
   step A, `await` A first, then build B (see the Update test in
   [`SaveClientCommandHandlerTests`](Features/Client/SaveClient/SaveClientCommandHandlerTests.cs)).

---

## 4. Act

One private `Act(...)` per test class. Resolve the component from the stage and call it — nothing else:

```csharp
private Task<SaveClientCommandResult> Act(SaveClientCommand command)
    => Stage.ExecuteAsync<IRequestHandler<SaveClientCommand, SaveClientCommandResult>, SaveClientCommandResult>(
           handler => handler.Handle(command).AsTask());
```

---

## 5. Inspects

Inspect helpers are **extension methods on `ITestInspect`** (or `ITestInspectScope<T>`).

- **Result assertions use a value scope.** Open it with `ThenValueScope(result, …)` so chained
  checks run inside a single `Assert.Multiple`, then chain `.IsSuccess(...)` / `.IsFailure<T>()` /
  `.Found(...)` (see [`InspectSaveClient`](Features/Client/SaveClient/InspectSaveClient.cs),
  [`InspectGetClient`](Features/Client/GetClient/InspectGetClient.cs),
  [`InspectValidation`](Helpers/InspectValidation.cs)).
- **State assertions read the real database** via `host.ExecuteAsync<ExampleContext>(…)` and expose an
  `out Capture<T>` so the entity can be snapshotted (see
  [`Helpers/InspectClient.cs`](Helpers/InspectClient.cs): `DbClientById`, `NoClientsInDb`).
- **Interaction assertions verify mocks**: `EventPublished` / `NoEventsPublished`, `CacheUpdated` /
  `CacheNotUpdated`.
- After a value scope's chained `.Then` calls, you are back on `ITestInspect` and can chain
  state/interaction inspects on the same line.

---

## 6. Snapshots (Verify)

Whole-entity assertions use Verify; targeted assertions stay explicit — the example uses both.

- `Inspect.Verify(capture)` snapshots a captured value with the project-wide settings in
  [`Helpers/VerifierSetup.cs`](Helpers/VerifierSetup.cs).
- Because ids and timestamps are arranged deterministically, snapshots are stable. GUIDs are shown
  literally (`DontScrubGuids`); `DateTime`s are scrubbed to ordered placeholders (`DateTime_1`,
  `DateTime_2`) which is enough to prove relationships (e.g. `CreatedAt` preserved while `UpdatedAt`
  advances on update).
- First run writes `*.received.txt` and the test "fails"; review it, then accept by renaming to
  `*.verified.txt` (or use the clipboard accept) and commit it.

---

## 7. Determinism

Handlers depend on `IDateTimeProvider.UtcNow` and `IIdGenerator.NewId()` instead of `DateTime.UtcNow`
/ `Guid.NewGuid()`. Arrange both with `Clock(...)` and `Ids(...)` so results are exactly assertable.
`ArrangeClient.FixedUtcNow` and `ArrangeClient.FixedClientId` are the shared fixed values.

---

## 8. Layout & naming

```
BaseIntegrationTest.cs                 # app composition: real Postgres + mocked infra
CONVENTIONS.md                         # this file
Helpers/
  VerifierSetup.cs                     # Verify settings
  ArrangeClient.cs / InspectClient.cs  # cross-feature building blocks (root namespace)
  InspectValidation.cs                 # FluentValidation result inspects
Features/Client/<Feature>/
  Arrange<Feature>.cs  Inspect<Feature>.cs
  <Handler>Tests.cs    <Validator>Tests.cs
  *.verified.txt
```

- Cross-feature helpers live in the **root** namespace `Mokkit.Example1.Integration.Tests` so they are
  in scope for every feature test via enclosing-namespace lookup.
- Test names follow `Operation_Condition_ExpectedOutcome`.
- The `Client` namespace segment shadows the `Client` entity type; alias it
  (`using ClientEntity = …Domain.Entities.Client;`) in files under `Features/Client/**` that name the
  entity. `Mokkit.Capture` collides with `Moq.Capture`; alias `using Capture = Mokkit.Capture;` where
  both are used.

---

## 9. Running

```bash
docker compose up -d postgres
dotnet test src/Mokkit.Example1.Integration.Tests
```

Each test starts from a Respawn-cleaned database, so tests are independent and order-free.
