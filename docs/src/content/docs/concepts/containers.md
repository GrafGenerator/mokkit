---
title: Containers & the mock→DI bridge
description: How Mokkit composes real services with test doubles so the code under test and your test share the same mock.
---

A [Stage](/concepts/stage/) is composed from **containers**. Mokkit doesn't ship its own DI or mocking
framework — it *adapts* the ones you already use, and its one real trick is making them cooperate: the mock a
test arranges is the very same instance the real service under test calls.

## Two roles a container plays

- **Mock containers** hold your test doubles — Moq mocks, NSubstitute substitutes, FakeItEasy fakes.
- **DI containers** compose the *real* application — Microsoft DI, Autofac, Castle Windsor.
- The dependency-free **Bag** is a third option: it just holds a few instances you hand it, no framework at
  all (great for a first test or a small SUT).

You pass the builders for these to `TestStageSetup.Create(...)`, and they're composed together into one Stage.

## The mock→DI bridge

The problem: your real `SaveClientHandler` asks its DI container for an `IClientCacheService`. Your test wants
that to be *its* mock — the one it sets up and later verifies. The bridge connects the two.

On the DI side you register the dependency with **`ResolveFromStage<T>()`** instead of a real implementation:

```csharp
services.ResolveFromStage<IClientCacheService>();
```

That registers a factory that, when the DI container is asked for `IClientCacheService`, reaches into the
Stage and returns the mock the mock-container deposited there (via `IStageResolve`, backed by the Stage's
shared bag). So the real handler and the test resolve the **same** object.

Usually you don't list dependencies one by one — you bridge *every* registered mock in a loop:

```csharp
// From the unit suite's BaseStageFixture: a substitute container + a Microsoft DI container.
var substitutes = new SubstituteContainerBuilder()
    .UseInit(s => { ConfigureSubstitutes(s); return Task.CompletedTask; });

var services = new ServiceProviderContainerBuilder()
    .UseInit(s =>
    {
        s.AddScoped<ILogger, NullLogger>();
        ConfigureServices(s);                    // register the real SUT
        return Task.CompletedTask;
    })
    .UsePreBuild<ISubstituteCollection>(InjectSubstitutes);   // ← bridge, see below

var setup = await TestStageSetup.Create(substitutes, services);

// Bridge every substitute into DI so the real SUT gets the test's doubles.
static Task InjectSubstitutes(IServiceCollection services, ISubstituteCollection substitutes)
{
    foreach (var registration in substitutes.Registrations)
        services.ResolveFromStage(registration.InnerType);
    return Task.CompletedTask;
}
```

The `UsePreBuild<ISubstituteCollection>(...)` hook is the one moment the four-phase build matters: **PreBuild**
is where the DI builder gets to *see* the mock container's registrations, so it can wire a `ResolveFromStage`
for each. After that, the composition is built and every stage entered from it shares the arrangement.

The result reads exactly the way you'd want:

```csharp
// ARRANGE the mock ...
await Arrange.Then(host => host.Execute<ISubstitute<IClientCacheService>>(m =>
    m.Value.GetClient(id).Returns(cached)));

// ... ACT runs the REAL handler, which resolves that same mock from DI ...
var result = await Act.GetClient(query);

// ... INSPECT verifies against it.
await Inspect.CacheNotUpdated();
```

## The adapters

Everything above works with whichever pair you prefer. Each adapter is a small package:

| Role | Packages |
| --- | --- |
| Mock library | `Mokkit.Containers.Moq` · `.NSubstitute` · `.FakeItEasy` |
| DI container | `Mokkit.Containers.Microsoft.Extensions.DependencyInjection` · `.Autofac` · `.CastleWindsor` |
| Dependency-free | `Mokkit.Containers.Bag` |
| Shared contracts | `Mokkit.Containers.Common` (referenced transitively) |

The suites in the [example](https://github.com/GrafGenerator/mokkit/tree/main/example/Example1) deliberately
use *different* stacks — NSubstitute + MS-DI for units, Moq + MS-DI for integration, Bag for E2E — to prove
the test body never depends on the choice.

:::tip[Rolling your own]
The adapter contract (`IDependencyContainerBuilder` → `IDependencyContainer`) is small. If your stack isn't
covered, you can write an adapter — see [Write a custom container adapter](/guides/custom-container-adapter/),
which walks through `SubstituteContainerBuilder`.
:::

## Next

- **[The Stage & lifecycle](/concepts/stage/)** — where these containers are composed and entered.
- **[Guides](/quickstart/)** — pick a mock library, wire a real DI container, bridge mocks into it.
