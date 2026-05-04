using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Mokkit.Arrange;
using Mokkit.Example1.Domain.Entities;
using NSubstitute.ExceptionExtensions;

namespace Mokkit.Example1.Unit.Tests;

/// <summary>
/// Arrange building blocks that shape what the substituted <see cref="IDistributedCache"/> returns.
/// Note: <c>GetStringAsync</c>/<c>SetStringAsync</c> are extension methods, so we configure the real
/// interface members (<c>GetAsync</c>/<c>SetAsync</c>) that they delegate to.
/// </summary>
public static class ArrangeCache
{
    public static string KeyFor(Guid clientId) => $"client:{clientId}";

    /// <summary>Arranges a cache hit: the key resolves to the serialized <paramref name="client"/>.</summary>
    public static ITestArrange CacheReturns(this ITestArrange arrange, Client client)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(client));

        return arrange.Then(host =>
        {
            host.Execute<IDistributedCache>(cache =>
                cache.GetAsync(KeyFor(client.Id), Arg.Any<CancellationToken>()).Returns(bytes));
        });
    }

    /// <summary>Arranges a cache miss for every key.</summary>
    public static ITestArrange CacheEmpty(this ITestArrange arrange)
    {
        return arrange.Then(host =>
        {
            host.Execute<IDistributedCache>(cache =>
                cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null));
        });
    }

    /// <summary>Arranges the cache read to fail, exercising the service's graceful-degradation path.</summary>
    public static ITestArrange CacheThrowsOnGet(this ITestArrange arrange)
    {
        return arrange.Then(host =>
        {
            host.Execute<IDistributedCache>(cache =>
                cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Throws(new InvalidOperationException("cache unavailable")));
        });
    }
}
