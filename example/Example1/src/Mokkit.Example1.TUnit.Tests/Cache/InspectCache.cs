using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Mokkit.Example1.Domain.Entities;
using Mokkit.Inspect;

namespace Mokkit.Example1.TUnit.Tests.Cache;

/// <summary>
/// Inspect building blocks that verify how the service interacted with the faked cache. Value assertions use
/// Shouldly (identical to the xUnit suite — only the framework changed); interaction assertions use FakeItEasy's
/// <c>MustHaveHappened</c> / <c>MustNotHaveHappened</c>.
/// </summary>
public static class InspectCache
{
    private static readonly TimeSpan ExpectedExpiration = TimeSpan.FromMinutes(30);

    /// <summary>Asserts the retrieved client equals the expected one (deep value comparison).</summary>
    public static ITestInspect RetrievedClientMatching(this ITestInspect inspect, Client? result, Client expected)
    {
        return inspect.Then(_ => result.ShouldBeEquivalentTo(expected));
    }

    /// <summary>Asserts nothing was retrieved (cache miss / degraded read).</summary>
    public static ITestInspect RetrievedNothing(this ITestInspect inspect, Client? result)
    {
        return inspect.Then(_ => result.ShouldBeNull());
    }

    /// <summary>Verifies the cache was read once for the client's key.</summary>
    public static ITestInspect CacheQueried(this ITestInspect inspect, Guid clientId)
    {
        return inspect.Then(host =>
        {
            host.Execute<IDistributedCache>(cache =>
                A.CallTo(() => cache.GetAsync(ArrangeCache.KeyFor(clientId), A<CancellationToken>._))
                    .MustHaveHappenedOnceExactly());
        });
    }

    /// <summary>Verifies the client was written to the cache, serialized, with the 30-minute expiry.</summary>
    public static ITestInspect CacheStored(this ITestInspect inspect, Client expected)
    {
        var expectedJson = JsonSerializer.Serialize(expected);

        return inspect.Then(host =>
        {
            host.Execute<IDistributedCache>(cache =>
                A.CallTo(() => cache.SetAsync(
                        ArrangeCache.KeyFor(expected.Id),
                        A<byte[]>.That.Matches(b => Encoding.UTF8.GetString(b) == expectedJson),
                        A<DistributedCacheEntryOptions>.That.Matches(o => o.AbsoluteExpirationRelativeToNow == ExpectedExpiration),
                        A<CancellationToken>._))
                    .MustHaveHappenedOnceExactly());
        });
    }

    /// <summary>Verifies the client's key was removed from the cache.</summary>
    public static ITestInspect CacheRemoved(this ITestInspect inspect, Guid clientId)
    {
        return inspect.Then(host =>
        {
            host.Execute<IDistributedCache>(cache =>
                A.CallTo(() => cache.RemoveAsync(ArrangeCache.KeyFor(clientId), A<CancellationToken>._))
                    .MustHaveHappenedOnceExactly());
        });
    }

    /// <summary>Verifies nothing was ever written to the cache.</summary>
    public static ITestInspect NothingStored(this ITestInspect inspect)
    {
        return inspect.Then(host =>
        {
            host.Execute<IDistributedCache>(cache =>
                A.CallTo(() => cache.SetAsync(
                        A<string>._,
                        A<byte[]>._,
                        A<DistributedCacheEntryOptions>._,
                        A<CancellationToken>._))
                    .MustNotHaveHappened());
        });
    }
}
