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

Mokkit ships only a Moq container. Swapping in NSubstitute is just implementing
`IDependencyContainerBuilder` — see [Containers/](Containers/):

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

## 2. Harness (xUnit lifecycle)

- [`StageFixture`](StageFixture.cs) (`IAsyncLifetime`) builds the stage once: a
  `SubstituteContainerBuilder` (registers `IDistributedCache`) + a `ServiceProviderContainerBuilder`
  composing the real classes under test (`AddApplicationLayer()`, the real `ClientCacheService`,
  `NullLogger`) and bridging substitutes with `ResolveFromStage`. No database is registered.
- [`BaseUnitTest`](BaseUnitTest.cs) uses `IClassFixture<StageFixture>`, enters a fresh stage per test in
  its constructor, and disposes it (`IDisposable`). Exposes `Stage`, `Arrange`, `Inspect`.
- Test classes are plain xUnit (`[Fact]` / `[Theory]` + `[InlineData]`), constructor injects the fixture.

---

## 3. Arrange / Act / Inspect with the new stack

Same shape as integration — extension methods on `ITestArrange` / `ITestInspect`:

- Arranges configure the substitute ([`ArrangeCache`](ArrangeCache.cs)): `CacheReturns`, `CacheEmpty`,
  `CacheThrowsOnGet`. Note `GetStringAsync`/`SetStringAsync` are **extension** methods NSubstitute can't
  intercept, so we configure the real members `GetAsync`/`SetAsync` they delegate to.
- Act resolves the class under test and calls it: `Stage.ExecuteAsync<IClientCacheService, Client?>(...)`.
- Inspects verify interactions ([`InspectCache`](InspectCache.cs)): `CacheQueried`, `CacheStored`,
  `CacheRemoved`, `NothingStored` — using NSubstitute `Received`/`DidNotReceive`.
- Result assertions use Shouldly directly (no value-scope / `Assert.Multiple` needed):
  `result.ShouldBeEquivalentTo(client)`.

---

## 4. What's tested (no database)

- [`ClientCacheServiceTests`](Cache/ClientCacheServiceTests.cs) — the real `ClientCacheService` with a
  substituted `IDistributedCache`: cache hit/miss/throw on read, serialize-and-store with the 30-minute
  expiry, and remove.
- [`SaveClientCommandValidatorTests`](Validation/SaveClientCommandValidatorTests.cs) — the validator
  resolved from the stage; `[Theory]`-driven rule coverage with Shouldly.

Handlers depend on `ExampleContext` (EF), so they belong to the integration suite, not here.

---

## 5. Running

```bash
dotnet test src/Mokkit.Example1.Unit.Tests   # no Docker / Postgres required
```
