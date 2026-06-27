using Mokkit.Example1.Application.Logic.Persistence;
using Mokkit.Example1.Domain.Entities;

namespace Mokkit.Example1.Unit.Tests.Cache;

/// <summary>
/// Unit tests for the real <c>ClientCacheService</c> (resolved from the stage) with a substituted
/// <c>IDistributedCache</c>. Every step is a business-named arrange/inspect; the SUT and its dependency
/// both come from the stage.
/// </summary>
public sealed class ClientCacheServiceTests : BaseUnitTest<CacheServiceFixture>
{
    public ClientCacheServiceTests(CacheServiceFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetClient_WhenCached_ReturnsDeserializedClient()
    {
        // ARRANGE
        await Arrange.CacheHasClient(out var client);

        // ACT
        var result = await GetClient(client.Value!.Id);

        // INSPECT
        await Inspect
            .RetrievedClientMatching(result, client.Value!)
            .CacheQueried(client.Value!.Id);
    }

    [Fact]
    public async Task GetClient_WhenMiss_ReturnsNull()
    {
        // ARRANGE
        var clientId = Guid.NewGuid();
        await Arrange.CacheHasNoClient();

        // ACT
        var result = await GetClient(clientId);

        // INSPECT
        await Inspect
            .RetrievedNothing(result)
            .CacheQueried(clientId);
    }

    [Fact]
    public async Task GetClient_WhenCacheThrows_DegradesToNull()
    {
        // ARRANGE
        await Arrange.CacheReadFails();

        // ACT
        var result = await GetClient(Guid.NewGuid());

        // INSPECT
        await Inspect.RetrievedNothing(result);
    }

    [Fact]
    public async Task SetClient_SerializesAndStoresWithExpiry()
    {
        // ARRANGE
        await Arrange.AClient(out var client);

        // ACT
        await StoreClient(client.Value!);

        // INSPECT
        await Inspect.CacheStored(client.Value!);
    }

    [Fact]
    public async Task RemoveClient_RemovesKey()
    {
        // ARRANGE
        var clientId = Guid.NewGuid();

        // ACT
        await RemoveClient(clientId);

        // INSPECT
        await Inspect.CacheRemoved(clientId);
    }

    private Task<Client?> GetClient(Guid clientId) =>
        Stage.ExecuteAsync<IClientCacheService, Client?>(cache => cache.GetClientAsync(clientId));

    private Task StoreClient(Client client) =>
        Stage.ExecuteAsync<IClientCacheService>(cache => cache.SetClientAsync(client));

    private Task RemoveClient(Guid clientId) =>
        Stage.ExecuteAsync<IClientCacheService>(cache => cache.RemoveClientAsync(clientId));
}
