using Mokkit.Example1.Application.Features.Client.GetClient;
using Mokkit.Example1.Common;

namespace Mokkit.Example1.Integration.Tests.Features.Client.GetClient;

[TestFixture]
public sealed class GetClientQueryHandlerTests : BaseIntegrationTest
{
    [Test]
    public async Task CacheHit_ReturnsClientFromCache_WithoutReCaching()
    {
        // ARRANGE — the client lives in the cache; the database is left empty.
        await Arrange
            .CachedClient(out var cached);

        // ACT
        var result = await Act(new GetClientQuery { ClientId = cached.Value!.Id });

        // INSPECT — served from cache, so the handler does not write the cache again.
        await Inspect
            .GetResult(result).Found(cached.Value!.Id)
            .CacheNotUpdated();
    }

    [Test]
    public async Task CacheMiss_DbHit_ReturnsFromDatabase_AndFillsCache()
    {
        // ARRANGE — empty cache, but the client exists in the database.
        await Arrange
            .EmptyCache()
            .DbClient(out var seeded);

        // ACT
        var result = await Act(new GetClientQuery { ClientId = seeded.Value!.Id });

        // INSPECT — served from the database and written back into the cache.
        await Inspect
            .GetResult(result).Found(seeded.Value!.Id)
            .CacheUpdated(seeded.Value!.Id);
    }

    [Test]
    public async Task NotFound_WhenAbsentFromCacheAndDatabase()
    {
        // ARRANGE — nothing cached, nothing seeded.
        await Arrange
            .EmptyCache();

        // ACT
        var result = await Act(new GetClientQuery { ClientId = ArrangeClient.FixedClientId });

        // INSPECT
        await Inspect
            .GetResult(result).NotFound()
            .CacheNotUpdated();
    }

    private Task<GetClientQueryResult> Act(GetClientQuery query)
        => Stage.ExecuteAsync<IRequestHandler<GetClientQuery, GetClientQueryResult>, GetClientQueryResult>(
            handler => handler.Handle(query).AsTask());
}
