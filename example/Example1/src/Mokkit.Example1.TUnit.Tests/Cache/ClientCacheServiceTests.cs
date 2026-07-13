using Mokkit.Example1.Application.Logic.Persistence;
using Mokkit.Example1.Domain.Entities;
using Mokkit.Suite;

namespace Mokkit.Example1.TUnit.Tests.Cache;

/// <summary>
/// The same <c>ClientCacheService</c> tests as the xUnit suite, running under <b>TUnit</b>. The bodies are
/// byte-for-byte the Mokkit Arrange / Act / Inspect flow; only the framework wiring differs — the fixture is
/// injected with <c>[ClassDataSource]</c> and each test is a <c>[Test]</c>, with the fresh stage entered by
/// the base's <c>[Before(Test)]</c> hook.
/// </summary>
public sealed class ClientCacheServiceTests : TUnitTestBase
{
    [ClassDataSource<CacheServiceFixture>(Shared = SharedType.PerClass)]
    public required CacheServiceFixture Fixture { get; init; }

    protected override TestStage EnterStage() => Fixture.EnterStage();

    [Test]
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

    [Test]
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

    [Test]
    public async Task GetClient_WhenCacheThrows_DegradesToNull()
    {
        // ARRANGE
        await Arrange.CacheReadFails();

        // ACT
        var result = await GetClient(Guid.NewGuid());

        // INSPECT
        await Inspect.RetrievedNothing(result);
    }

    [Test]
    public async Task SetClient_SerializesAndStoresWithExpiry()
    {
        // ARRANGE
        await Arrange.AClient(out var client);

        // ACT
        await StoreClient(client.Value!);

        // INSPECT
        await Inspect.CacheStored(client.Value!);
    }

    [Test]
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
