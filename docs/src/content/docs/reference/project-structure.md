---
title: How to structure a Mokkit test project
description: The layout that makes vocabulary discoverable and reusable — colocated verbs, one fixture per SUT, and the discipline that keeps AAA honest.
---

Mokkit doesn't prescribe a folder layout, but a good one makes your [vocabulary](/concepts/vocabulary/)
discoverable and your fixtures obvious. This is the structure the example uses across three suites.

## Colocate vocabulary with what it describes

Keep the verbs for a feature (or system-under-test) next to that feature's tests, in the same namespace — so
they're in scope with no extra `using`s. The canonical unit of organisation is a folder holding four files:

```
Features/Client/SaveClient/
├── ArrangeSaveClient.cs     # arrange verbs   (build commands, seed state)
├── ActSaveClient.cs         # act verbs        (the operation under test)
├── InspectSaveClient.cs     # inspect verbs    (observe results & effects)
└── SaveClientCommandHandlerTests.cs
```

`Arrange<Feature>.cs` / `Act<Feature>.cs` / `Inspect<Feature>.cs` mirror the three phases. A test in this folder
reads as `await Arrange.…`, `await Act.…`, `await Inspect.…` using words defined right beside it.

Genuinely cross-feature verbs (a `Clock`/`Ids` arrange, a shared `DbClient` seed) go in a **root-namespace**
`Helpers/` folder, so enclosing-namespace lookup makes them visible to every feature test.

## A base fixture builds the Stage once

Put the [Stage](/concepts/stage/) composition in a base fixture that builds **once** and exposes the three phases
as properties:

```csharp
protected TestStage Stage { get; }
protected ITestArrange Arrange => Stage.Arrange();
protected ITestAct     Act     => Stage.Act();
protected ITestInspect Inspect => Stage.Inspect();
```

Each test enters a fresh stage over that fixed composition and disposes it afterwards. This is identical across
xUnit / NUnit / MSTest / TUnit — only the fixture attributes differ.

## One container build → one fixture per SUT

Because containers are built once, a type is either the *real* SUT or a *mock* in a given composition — not
both. So each system-under-test gets **its own small fixture**, declaring its real type plus mocks for its
direct dependencies:

```csharp
public sealed class ProcessorFixture : BaseStageFixture
{
    protected override void ConfigureSubstitutes(ISubstituteCollection s)
    {
        s.AddSubstitute<IRequestHandler<SaveClientCommand, SaveClientCommandResult>>();
        s.AddSubstitute<IKafkaEventPublisher>();
    }
    protected override void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<IClientStatusChangedProcessor, ClientStatusChangedProcessor>();
}
```

## The discipline that keeps it honest

One rule underpins the whole layout:

> **Arrange and Act *produce* artifacts. Inspect only *observes* them** — reads are fine, mutations are not.

No raw setup or assertions in a test body: every value is a business-named arrange, every check a business-named
inspect, and the act returns (or captures) the artifact the inspects observe. If an inspect is doing the thing
under test, lift it into Act; if an act asserts, move that into Inspect.

## Four real layouts

The example organises each suite by whatever axis fits it — all following the rules above:

```
Unit.Tests/            # organised BY SUT/concern (portability proof: xUnit + NSubstitute)
├── BaseStageFixture.cs  BaseUnitTest.cs
├── Containers/                     # a bring-your-own NSubstitute adapter
├── Cache/          { CacheServiceFixture, ArrangeCache, InspectCache, …Tests }
├── Validation/     { ValidatorFixture, ArrangeCommand, InspectValidation, …Tests }
└── Messaging/                      # one concern, several SUTs → subfolder per SUT
    ├── Processor/   { ProcessorFixture, ArrangeProcessor, InspectProcessor, …Tests }
    ├── Publisher/   { … }
    └── Consumer/    { … }

Integration.Tests/    # organised BY FEATURE (NUnit + Moq + Postgres/Respawn)
├── BaseIntegrationTest.cs
├── Helpers/         { ArrangeClient, InspectClient, VerifierSetup }   # cross-feature, root namespace
└── Features/Client/
    ├── GetClient/   { ArrangeGetClient, ActGetClient, InspectGetClient, …Tests }
    └── SaveClient/  { ArrangeSaveClient, ActSaveClient, InspectSaveClient, …Tests, *.verified.txt }

E2E.Tests/            # organised BY EXTERNAL SURFACE (xUnit + Testcontainers, nothing mocked)
├── BaseE2ETest.cs  E2EStack.cs  E2ECollection.cs  KafkaProbe.cs  Poll.cs
├── Clients/         { ClientApi, ArrangeClientApi, ArrangeMessages, ActClientApi,
│                      InspectClientApi, *FlowTests, ClientLifecycleScenarioTests }
└── Contracts/       # suite-owned wire DTOs — not the service's internal types

TUnit.Tests/          # PORTABILITY PROOF #2 (TUnit + FakeItEasy · Microsoft.Testing.Platform · dotnet run)
├── TUnitTestBase.cs                # [Before(Test)]/[After(Test)] instead of IClassFixture
└── Cache/           { CacheServiceFixture, ArrangeCache, InspectCache, …Tests }   # same Cache tests, new stack
```

Unit organises **by SUT** (one fixture each), integration **by feature** (cross-feature helpers hoisted to the
root), E2E **by external surface** (infra plumbing at the root, its own black-box contracts). The small
**TUnit** suite re-runs the unit `Cache` tests on a fourth stack (TUnit + FakeItEasy) to prove the Mokkit code
is unchanged. Same Mokkit primitives throughout — only the surrounding stack changes.

## Next

- **[Conventions cheat-sheet](/reference/conventions/)** — the one-screen version of this.
- **[Building your test vocabulary](/concepts/vocabulary/)** — what fills the `Arrange*`/`Act*`/`Inspect*` files.
