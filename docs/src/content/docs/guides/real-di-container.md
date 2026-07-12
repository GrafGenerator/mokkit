---
title: Wire a real DI container + bridge mocks
description: Compose the real application from your DI container while a mock container feeds test doubles into it.
---

Hand-wiring a service with the [Bag](/guides/bag-container/) is fine for a small SUT, but real code is usually
assembled by a DI container — and you want to test the *real* composition, with only the outermost
collaborators faked. Mokkit does this by running two containers side by side and **bridging** them: a mock
container holds your doubles, your real DI container composes the app, and each mocked dependency is registered
to resolve *from the stage*. This page shows the wiring; for the concept, see
[Containers & the mock→DI bridge](/concepts/containers/).

## The composition

The example's unit `BaseStageFixture` is the reusable pattern — a substitute container plus a Microsoft DI
container:

```csharp
public abstract class BaseStageFixture : IAsyncLifetime
{
    private TestStageSetup _setup = null!;

    public async Task InitializeAsync()
    {
        var substitutes = new SubstituteContainerBuilder()
            .UseInit(s => { ConfigureSubstitutes(s); return Task.CompletedTask; });

        var services = new ServiceProviderContainerBuilder()
            .UseInit(s =>
            {
                s.AddScoped<ILogger, NullLogger>();
                ConfigureServices(s);                          // register the REAL system-under-test
                return Task.CompletedTask;
            })
            .UsePreBuild<ISubstituteCollection>(InjectSubstitutes);   // ← the bridge

        _setup = await TestStageSetup.Create(substitutes, services);
    }

    public TestStage EnterStage() => _setup.EnterStage();

    protected abstract void ConfigureSubstitutes(ISubstituteCollection substitutes);
    protected abstract void ConfigureServices(IServiceCollection services);

    // Register every substitute so DI resolves it FROM THE STAGE instead of building a real one.
    private static Task InjectSubstitutes(IServiceCollection services, ISubstituteCollection substitutes)
    {
        foreach (var registration in substitutes.Registrations)
            services.ResolveFromStage(registration.InnerType);
        return Task.CompletedTask;
    }
}
```

Concrete fixtures then declare only *what differs* — the SUT and which of its dependencies are doubles:

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

## How the bridge works

`ResolveFromStage<T>()` (or the loop over `substitutes.Registrations`) registers a DI factory that, when asked
for `T`, reaches into the stage and returns the double the mock container deposited there:

```csharp
// Effectively what ResolveFromStage registers:
services.AddTransient(serviceType, sp =>
    sp.GetRequiredService<IStageResolve>().Resolve(serviceType)
        ?? throw new InvalidOperationException($"Cannot resolve {serviceType} from the stage"));
```

So the real `ClientStatusChangedProcessor`, resolved from DI, asks for `IKafkaEventPublisher` and gets **the
test's substitute** — the one arranged in `HandlerSucceedsFor` and checked in `ConfirmationPublishedFor`.

The `UsePreBuild<ISubstituteCollection>(...)` hook is the one moment ordering matters: **PreBuild** is when the
DI builder can see the mock container's registrations, so it can wire a bridge for each. (This is the only
phase of the [four-phase build](/concepts/stage/#the-4-phase-container-build) you interact with.)

## Swapping the stack

Nothing above is Microsoft-DI- or NSubstitute-specific. Substitute the builders and the rest is identical:

| Swap | For |
| --- | --- |
| `SubstituteContainerBuilder` (NSubstitute) | `MoqContainerBuilder` · `FakeItEasyContainerBuilder` |
| `ServiceProviderContainerBuilder` (MS-DI) | `AutofacContainerBuilder` · `CastleWindsorContainerBuilder` |

The integration suite, for instance, uses **Moq + Microsoft DI** with the exact same `ResolveFromStage` loop —
see [Integration-test against a real database](/guides/integration-database/).

## Next

- **[Containers & the mock→DI bridge](/concepts/containers/)** — the concept in full.
- **[Write a custom container adapter](/guides/custom-container-adapter/)** — if your stack isn't covered.
