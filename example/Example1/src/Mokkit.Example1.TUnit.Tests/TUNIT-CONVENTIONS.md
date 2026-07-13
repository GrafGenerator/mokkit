# Mokkit TUnit-test conventions (a fourth stack)

This suite exists to prove one thing: **the Mokkit Arrange / Act / Inspect flow is identical no matter the
test framework or mock library.** It re-runs the unit suite's `ClientCacheService` tests on a *completely*
different stack — **TUnit** instead of xUnit, **FakeItEasy** instead of NSubstitute — with the Mokkit code
unchanged.

| Concern | Unit suite | **This suite** |
|---|---|---|
| Test framework | xUnit (VSTest) | **TUnit** (Microsoft.Testing.Platform) |
| Mocking | NSubstitute (custom container) | **FakeItEasy** (`Mokkit.Containers.FakeItEasy`) |
| DI / bridge | Microsoft DI + `ResolveFromStage` | **same** |
| Assertions | Shouldly | Shouldly (values) + FakeItEasy (interactions) |
| Infrastructure | none | **none** — pure unit, one faked dependency |

The `Cache/` tests, arranges and inspects are byte-for-byte the same Mokkit calls as
[`Unit.Tests/Cache/`](../Mokkit.Example1.Unit.Tests/Cache/) — compare them side by side.

---

## 1. TUnit ≠ VSTest — how this project is wired

TUnit runs on **Microsoft.Testing.Platform (MTP)**, so the project is a console **executable**, not a VSTest
library. That means the csproj differs from the other suites:

- `<OutputType>Exe</OutputType>` is **required** (omit it → `hostfxr.dll could not be found`).
- It must **not** reference `Microsoft.NET.Test.Sdk` or `coverlet.collector` (they break MTP discovery).
- The `TUnit` package pulls in the runner, core and assertions.

## 2. Lifecycle — the only Mokkit-adjacent code that changes

TUnit has no `IClassFixture`/`IAsyncLifetime`. The mapping to the same "build once, fresh stage per test"
shape ([`TUnitTestBase`](TUnitTestBase.cs) + [`CacheServiceFixture`](Cache/CacheServiceFixture.cs)):

- **Composition, once per class** — the fixture implements TUnit's `IAsyncInitializer` (`InitializeAsync`) +
  `IAsyncDisposable`, and the test class injects it with `[ClassDataSource<CacheServiceFixture>(Shared = SharedType.PerClass)]`.
- **Fresh stage per test** — the base's `[Before(Test)]` hook calls `Fixture.EnterStage()`; `[After(Test)]`
  disposes it. Base-class hooks are inherited, so concrete classes stay clean.
- Tests are `[Test]` methods; the bodies are ordinary `await Arrange… / await …Act… / await Inspect…`.

`TestStageSetup.Create(...)` / `EnterStage()` / `Stage.Dispose()` are **identical** to the other suites.

## 3. FakeItEasy through the stage

The fake is registered with `Mokkit.Containers.FakeItEasy` (`fakeCollection.AddFake<IDistributedCache>()`) and
bridged into the real Microsoft DI graph with `ResolveFromStage` (same loop the integration suite uses for
Moq). Because a FakeItEasy fake **is** the interface, the fake the arrange configures, the dependency the real
`ClientCacheService` receives, and the handle the inspect verifies are the **same object** — configured with
`A.CallTo(() => cache.GetAsync(…)).Returns(…)` and verified with `.MustHaveHappenedOnceExactly()`.

(`GetStringAsync`/`SetStringAsync` are extension methods FakeItEasy can't intercept, so the arranges/inspects
target the real members `GetAsync`/`SetAsync` — same note as the NSubstitute version.)

## 4. Running

MTP and VSTest can't share a `dotnet test` run, so run this suite on its **own** — no Docker/DB needed:

```bash
dotnet run --project src/Mokkit.Example1.TUnit.Tests
```

(The other suites keep running as before with `dotnet test src/Mokkit.Example1.<Unit|Integration|E2E>.Tests`.)
Do **not** run `dotnet test` over the whole solution — mixing MTP (this suite) and VSTest (the others) in one
run is unsupported.
