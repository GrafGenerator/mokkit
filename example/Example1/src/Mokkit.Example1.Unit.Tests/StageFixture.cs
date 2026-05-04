using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Mokkit.Containers;
using Mokkit.Containers.Microsoft.Extensions.DependencyInjection;
using Mokkit.Example1.Application;
using Mokkit.Example1.Application.Logic.Persistence;
using Mokkit.Example1.Infrastructure.Logic.Cache;
using Mokkit.Example1.Unit.Tests.Containers;
using Mokkit.Suite;

namespace Mokkit.Example1.Unit.Tests;

/// <summary>
/// Builds the Mokkit stage once for the unit suite. Unlike the integration base, there is no database:
/// the only outward dependency (<see cref="IDistributedCache"/>) is an NSubstitute fake provided by our
/// custom <see cref="SubstituteContainerBuilder"/>, and the classes under test (the validator and the
/// real <c>ClientCacheService</c>) are composed in the Microsoft DI container.
/// </summary>
public sealed class StageFixture : IAsyncLifetime
{
    private TestStageSetup _setup = null!;

    public async Task InitializeAsync()
    {
        var substituteContainerBuilder = new SubstituteContainerBuilder()
            .UseInit(BuildSubstitutes);

        var serviceProviderContainerBuilder = new ServiceProviderContainerBuilder()
            .UseInit(BuildServices)
            .UsePreBuild<ISubstituteCollection>(InjectSubstitutes);

        _setup = await TestStageSetup.Create(new IDependencyContainerBuilder[]
        {
            substituteContainerBuilder,
            serviceProviderContainerBuilder,
        });
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>Enters a fresh, isolated stage (new substitutes) for a single test.</summary>
    public TestStage EnterStage() => _setup.EnterStage();

    private static Task BuildSubstitutes(ISubstituteCollection substitutes)
    {
        substitutes.AddSubstitute<IDistributedCache>();
        return Task.CompletedTask;
    }

    private static Task BuildServices(IServiceCollection services)
    {
        services.AddScoped<ILogger, NullLogger>();
        services.AddScoped(typeof(ILogger<>), typeof(NullLogger<>));

        // Real classes under test — no database is registered, so handlers (which need
        // ExampleContext) are simply never resolved here.
        services.AddApplicationLayer();
        services.AddScoped<IClientCacheService, ClientCacheService>();

        return Task.CompletedTask;
    }

    private static Task InjectSubstitutes(IServiceCollection services, ISubstituteCollection substitutes)
    {
        foreach (var registration in substitutes.Registrations)
        {
            services.ResolveFromStage(registration.InnerType);
        }

        return Task.CompletedTask;
    }
}
