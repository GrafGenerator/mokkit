---
title: The Stage & lifecycle
description: Where your services live during a test — how a Stage is composed once and entered fresh for every test.
---

The **Stage** is the runtime a test runs against. It holds the services a test resolves — the real
system-under-test plus its (real or mocked) collaborators — and it's what `Arrange`, `Act` and `Inspect`
pull from. Everything else in Mokkit sits on top of it.

## Compose once, enter per test

A Stage comes in two steps:

1. **`TestStageSetup.Create(...builders)`** composes your [containers](/concepts/containers/) — this is the
   expensive part, and you do it **once**.
2. **`setup.EnterStage()`** returns a fresh, isolated `TestStage` — you do this **once per test**, and dispose
   it afterwards.

```csharp
// Once — usually in a class/collection fixture.
var setup = await TestStageSetup.Create(
    new BagContainerBuilder()
        .AddInstance(email)
        .AddInstance(new SignupService(email)));

// Per test.
var stage = setup.EnterStage();
// ... arrange / act / inspect ...
stage.Dispose();
```

Each `EnterStage()` opens its own scope, so tests are isolated: scoped services are created per stage and
disposed when the stage is disposed. Nothing leaks between tests.

## What a Stage gives you

```csharp
stage.Arrange();   // → ITestArrange — start the setup chain
stage.Act();       // → ITestAct     — start the act chain
stage.Inspect();   // → ITestInspect — start the observe chain

stage.Execute<TService>(svc => ...);              // resolve one service and run it
stage.ExecuteAsync<TService, TOut>(svc => ...);   // resolve, run, return a result
```

`Execute`/`ExecuteAsync` come in 1-to-4-service arities, so a step can pull several collaborators at once.
Your [vocabulary](/concepts/vocabulary/) verbs are thin wrappers over exactly these calls.

## Wiring it to your test framework

Mokkit is framework-agnostic — the Stage is composed in whatever "run once" hook your runner offers and
entered in its "per test" hook. The pattern is identical across xUnit, NUnit and MSTest; only the fixture
attributes differ.

```csharp
// xUnit — the composition is an IClassFixture (built once); each test enters a fresh stage.
public abstract class BaseUnitTest<TFixture> : IClassFixture<TFixture>, IDisposable
    where TFixture : BaseStageFixture
{
    protected BaseUnitTest(TFixture fixture) => Stage = fixture.EnterStage();

    protected TestStage Stage { get; }

    protected ITestArrange Arrange => Stage.Arrange();
    protected ITestAct     Act     => Stage.Act();
    protected ITestInspect Inspect => Stage.Inspect();

    public void Dispose() => Stage.Dispose();
}
```

Exposing `Arrange` / `Act` / `Inspect` as properties on a base fixture is what lets a test body read as
`await Arrange.…` / `await Act.…` / `await Inspect.…` with no ceremony.

## One composition per system-under-test

Because containers are built **once** per composition, a service is either the *real* thing or a *mock*
within a given Stage — not both. So a type that is the system-under-test in one test but a **dependency** in
another needs **its own fixture**:

> One fixture per SUT: its real type, plus mocks for that type's direct dependencies.

This is a feature, not a limitation — it keeps each test's composition small and obvious. The
[project structure](/reference/project-structure/) page shows how to organise fixtures per feature.

:::note[The 4-phase container build]
`Create` runs each builder through four phases — **PreInit → Init → PreBuild → Build** — before any stage is
entered. The only phase you normally notice is `PreBuild`, where a DI container gets to see the mock
container's registrations so it can [bridge them in](/concepts/containers/#the-mockdi-bridge). It's incidental
machinery; you configure builders, not phases.
:::

## Next

- **[Containers & the mock→DI bridge](/concepts/containers/)** — what goes into `Create`, and how mocks reach
  the real service.
- **[Captures: Capture vs Trapture](/concepts/captures/)** — how artifacts thread from one phase to the next.
