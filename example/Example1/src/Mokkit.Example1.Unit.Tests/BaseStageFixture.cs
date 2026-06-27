using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Mokkit.Containers;
using Mokkit.Containers.Microsoft.Extensions.DependencyInjection;
using Mokkit.Example1.Unit.Tests.Containers;
using Mokkit.Suite;

namespace Mokkit.Example1.Unit.Tests;

/// <summary>
/// Common plumbing for a unit-test stage: a substitute container (NSubstitute) + a Microsoft DI
/// container that registers <c>NullLogger</c> and bridges every substitute in via <c>ResolveFromStage</c>.
/// Each concrete fixture declares the composition for one system-under-test — its real type plus the
/// substitutes for that type's direct dependencies. (Mokkit builds containers once, so a type that is the
/// SUT in one test and a dependency in another needs its own fixture.)
/// </summary>
public abstract class BaseStageFixture : IAsyncLifetime
{
    private TestStageSetup _setup = null!;

    public async Task InitializeAsync()
    {
        var substituteContainerBuilder = new SubstituteContainerBuilder()
            .UseInit(substitutes =>
            {
                ConfigureSubstitutes(substitutes);
                return Task.CompletedTask;
            });

        var serviceProviderContainerBuilder = new ServiceProviderContainerBuilder()
            .UseInit(services =>
            {
                services.AddScoped<ILogger, NullLogger>();
                services.AddScoped(typeof(ILogger<>), typeof(NullLogger<>));
                ConfigureServices(services);
                return Task.CompletedTask;
            })
            .UsePreBuild<ISubstituteCollection>(InjectSubstitutes);

        _setup = await TestStageSetup.Create(substituteContainerBuilder, serviceProviderContainerBuilder);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>Enters a fresh, isolated stage (new substitutes) for a single test.</summary>
    public TestStage EnterStage() => _setup.EnterStage();

    /// <summary>Register the substitutes for this SUT's direct dependencies.</summary>
    protected abstract void ConfigureSubstitutes(ISubstituteCollection substitutes);

    /// <summary>Register the real system-under-test (and anything else it needs from real DI).</summary>
    protected abstract void ConfigureServices(IServiceCollection services);

    private static Task InjectSubstitutes(IServiceCollection services, ISubstituteCollection substitutes)
    {
        foreach (var registration in substitutes.Registrations)
        {
            services.ResolveFromStage(registration.InnerType);
        }

        return Task.CompletedTask;
    }
}
