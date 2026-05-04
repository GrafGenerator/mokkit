using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mokkit.Example1.Application.Logic.Persistence;
using Mokkit.Example1.Common;
using Mokkit.Example1.Db.Postgres;

namespace Mokkit.Example1.Application.Features.Client.GetClient;

/// <summary>
/// Handler for query <see cref="GetClientQuery"/>
/// </summary>
internal sealed class GetClientQueryHandler : IRequestHandler<GetClientQuery, GetClientQueryResult>
{
    private readonly ExampleContext _dbContext;
    private readonly IClientCacheService _cacheService;
    private readonly ILogger<GetClientQueryHandler> _logger;

    /// <summary>
    /// Handler for query <see cref="GetClientQuery"/>
    /// </summary>
    public GetClientQueryHandler(
        ExampleContext dbContext,
        IClientCacheService cacheService,
        ILogger<GetClientQueryHandler> logger)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async ValueTask<GetClientQueryResult> Handle(
        GetClientQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting client: {ClientId}", query.ClientId);

            // Try cache first
            var cachedClient = await _cacheService.GetClientAsync(query.ClientId, cancellationToken);
            if (cachedClient != null)
            {
                _logger.LogInformation("Client found in cache: {ClientId}", query.ClientId);
                return new GetClientQueryResult(true, cachedClient);
            }

            // Fallback to database
            _logger.LogInformation("Client not found in cache, checking database: {ClientId}", query.ClientId);
            var client = await _dbContext.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == query.ClientId, cancellationToken);

            if (client == null)
            {
                _logger.LogWarning("Client not found: {ClientId}", query.ClientId);
                return new GetClientQueryResult(false, null);
            }

            // Update cache for future requests
            await _cacheService.SetClientAsync(client, cancellationToken);
            
            _logger.LogInformation("Client found in database and cached: {ClientId}", query.ClientId);
            return new GetClientQueryResult(true, client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client: {ClientId}", query.ClientId);
            return new GetClientQueryResult(false, null, ex);
        }
    }
}
