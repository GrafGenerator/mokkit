using Mokkit.Arrange;
using Mokkit.Example1.Application.Logic.Persistence;
using Moq;
using Capture = Mokkit.Capture;
using ClientEntity = Mokkit.Example1.Domain.Entities.Client;

namespace Mokkit.Example1.Integration.Tests.Features.Client.GetClient;

/// <summary>
/// Arrange building blocks for the GetClient query — they shape what the cache returns.
/// </summary>
public static class ArrangeGetClient
{
    /// <summary>
    /// Arranges the cache to return a client for its id (a cache hit). The client is captured
    /// for assertions.
    /// </summary>
    public static ITestArrange CachedClient(
        this ITestArrange arrange,
        out Capture<ClientEntity> clientCapture,
        Action<ClientEntity>? mutate = null)
    {
        var capture = Capture.Start(out clientCapture);

        return arrange.Then(host =>
        {
            host.Execute<Mock<IClientCacheService>>(mock =>
            {
                var client = ArrangeClient.NewDefaultClient(mutate);

                mock.Setup(x => x.GetClientAsync(client.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(client);

                capture.Set(client);
            });
        });
    }

    /// <summary>
    /// Arranges the cache to be empty for every id (a cache miss), forcing a database read.
    /// </summary>
    public static ITestArrange EmptyCache(this ITestArrange arrange)
    {
        return arrange.Then(host =>
        {
            host.Execute<Mock<IClientCacheService>>(mock =>
                mock.Setup(x => x.GetClientAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ClientEntity?)null));
        });
    }
}
