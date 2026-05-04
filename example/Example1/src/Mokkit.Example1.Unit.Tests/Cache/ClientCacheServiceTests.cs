using Mokkit.Example1.Application.Logic.Persistence;
using Mokkit.Example1.Domain.Entities;

namespace Mokkit.Example1.Unit.Tests.Cache;

/// <summary>
/// Unit tests for the real <c>ClientCacheService</c> with a substituted <c>IDistributedCache</c>.
/// Same Mokkit Arrange/Act/Inspect flow as the integration suite, but on a wholly different stack
/// (xUnit + NSubstitute + Shouldly) and with no infrastructure.
/// </summary>
public sealed class ClientCacheServiceTests : BaseUnitTest
{
    public ClientCacheServiceTests(StageFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetClient_WhenCached_ReturnsDeserializedClient()
    {
        // ARRANGE
        var client = ClientFaker.NewClient();
        await Arrange.CacheReturns(client);

        // ACT
        var result = await Act(cache => cache.GetClientAsync(client.Id));

        // INSPECT
        result.ShouldNotBeNull();
        result.ShouldBeEquivalentTo(client);
        await Inspect.CacheQueried(client.Id);
    }

    [Fact]
    public async Task GetClient_WhenMiss_ReturnsNull()
    {
        // ARRANGE
        var clientId = Guid.NewGuid();
        await Arrange.CacheEmpty();

        // ACT
        var result = await Act(cache => cache.GetClientAsync(clientId));

        // INSPECT
        result.ShouldBeNull();
        await Inspect.CacheQueried(clientId);
    }

    [Fact]
    public async Task GetClient_WhenCacheThrows_DegradesToNull()
    {
        // ARRANGE — the cache is down; the service must swallow it and report a miss.
        await Arrange.CacheThrowsOnGet();

        // ACT
        var result = await Act(cache => cache.GetClientAsync(Guid.NewGuid()));

        // INSPECT
        result.ShouldBeNull();
    }

    [Fact]
    public async Task SetClient_SerializesAndStoresWithExpiry()
    {
        // ARRANGE
        var client = ClientFaker.NewClient();

        // ACT
        await Stage.ExecuteAsync<IClientCacheService>(cache => cache.SetClientAsync(client));

        // INSPECT
        await Inspect.CacheStored(client);
    }

    [Fact]
    public async Task RemoveClient_RemovesKey()
    {
        // ARRANGE
        var clientId = Guid.NewGuid();

        // ACT
        await Stage.ExecuteAsync<IClientCacheService>(cache => cache.RemoveClientAsync(clientId));

        // INSPECT
        await Inspect.CacheRemoved(clientId);
    }

    private Task<Client?> Act(Func<IClientCacheService, Task<Client?>> act)
        => Stage.ExecuteAsync<IClientCacheService, Client?>(act);
}
