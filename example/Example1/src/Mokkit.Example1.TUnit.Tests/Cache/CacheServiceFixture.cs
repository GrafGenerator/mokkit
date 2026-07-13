using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Mokkit.Containers.Common;
using Mokkit.Containers.FakeItEasy;
using Mokkit.Containers.Microsoft.Extensions.DependencyInjection;
using Mokkit.Example1.Application.Logic.Persistence;
using Mokkit.Example1.Infrastructure.Logic.Cache;
using Mokkit.Suite;
using TUnit.Core.Interfaces;

namespace Mokkit.Example1.TUnit.Tests.Cache;

/// <summary>
/// The stage composition, built once per test class (TUnit drives <see cref="IAsyncInitializer"/> /
/// <see cref="IAsyncDisposable"/> for a <c>[ClassDataSource]</c>). SUT: the real <c>ClientCacheService</c>;
/// dependency: a <b>FakeItEasy</b> fake <see cref="IDistributedCache"/>, bridged into the real Microsoft DI
/// graph via <c>ResolveFromStage</c> — the same mock→DI bridge the xUnit/NUnit suites use, different mock lib.
/// </summary>
public sealed class CacheServiceFixture : IAsyncInitializer, IAsyncDisposable
{
    private TestStageSetup _setup = null!;

    public async Task InitializeAsync()
    {
        var fakes = new FakeItEasyContainerBuilder()
            .UseInit(fakeCollection =>
            {
                fakeCollection.AddFake<IDistributedCache>();
                return Task.CompletedTask;
            });

        var services = new ServiceProviderContainerBuilder()
            .UseInit(collection =>
            {
                collection.AddScoped<ILogger, NullLogger>();
                collection.AddScoped(typeof(ILogger<>), typeof(NullLogger<>));
                collection.AddScoped<IClientCacheService, ClientCacheService>();
                return Task.CompletedTask;
            })
            .UsePreBuild<IMockCollection<object>>(InjectFakes);

        _setup = await TestStageSetup.Create(fakes, services);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>Enters a fresh, isolated stage (new fakes) for a single test.</summary>
    public TestStage EnterStage() => _setup.EnterStage();

    private static Task InjectFakes(IServiceCollection services, IMockCollection<object> fakes)
    {
        foreach (var registration in fakes.Registrations)
        {
            services.ResolveFromStage(registration.InnerType);
        }

        return Task.CompletedTask;
    }
}
