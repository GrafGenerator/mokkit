using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Mokkit.Arrange;
using Mokkit.Example1.Domain.Entities;
using Capture = Mokkit.Capture;

namespace Mokkit.Example1.TUnit.Tests.Cache;

/// <summary>
/// Arrange building blocks for the cache service: build the data to act on and shape what the faked
/// <see cref="IDistributedCache"/> returns. (<c>GetStringAsync</c>/<c>SetStringAsync</c> are extension methods
/// FakeItEasy can't intercept, so we configure the real members <c>GetAsync</c>/<c>SetAsync</c>.)
/// </summary>
public static class ArrangeCache
{
    public static string KeyFor(Guid clientId) => $"client:{clientId}";

    /// <summary>Builds and captures a client to act on (no cache interaction).</summary>
    public static ITestArrange AClient(
        this ITestArrange arrange,
        out Capture<Client> clientCapture,
        Action<Client>? mutate = null)
    {
        var capture = Capture.Start(out clientCapture);
        return arrange.Then(_ => capture.Set(ClientFaker.NewClient(mutate)));
    }

    /// <summary>Cache hit: the client's key resolves to its serialized form. The client is captured.</summary>
    public static ITestArrange CacheHasClient(
        this ITestArrange arrange,
        out Capture<Client> clientCapture,
        Action<Client>? mutate = null)
    {
        var capture = Capture.Start(out clientCapture);
        return arrange.Then(host =>
        {
            var client = ClientFaker.NewClient(mutate);
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(client));

            host.Execute<IDistributedCache>(cache =>
                A.CallTo(() => cache.GetAsync(KeyFor(client.Id), A<CancellationToken>._)).Returns(bytes));

            capture.Set(client);
        });
    }

    /// <summary>Cache miss for every key.</summary>
    public static ITestArrange CacheHasNoClient(this ITestArrange arrange)
    {
        return arrange.Then(host =>
        {
            host.Execute<IDistributedCache>(cache =>
                A.CallTo(() => cache.GetAsync(A<string>._, A<CancellationToken>._)).Returns((byte[]?)null));
        });
    }

    /// <summary>The cache read fails, exercising the service's graceful-degradation path.</summary>
    public static ITestArrange CacheReadFails(this ITestArrange arrange)
    {
        return arrange.Then(host =>
        {
            host.Execute<IDistributedCache>(cache =>
                A.CallTo(() => cache.GetAsync(A<string>._, A<CancellationToken>._))
                    .ThrowsAsync(new InvalidOperationException("cache unavailable")));
        });
    }
}
