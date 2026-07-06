using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Mokkit.Example1.Application.Logic.Persistence;
using Mokkit.Example1.Infrastructure.Logic.Cache;
using Mokkit.Example1.Unit.Tests.Containers;

namespace Mokkit.Example1.Unit.Tests.Cache;

/// <summary>SUT: the real <c>ClientCacheService</c>; dependency: a substituted <see cref="IDistributedCache"/>.</summary>
public sealed class CacheServiceFixture : BaseStageFixture
{
    protected override void ConfigureSubstitutes(ISubstituteCollection substitutes) =>
        substitutes.AddSubstitute<IDistributedCache>();

    protected override void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<IClientCacheService, ClientCacheService>();
}
