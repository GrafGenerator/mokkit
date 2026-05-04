using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Mokkit.Example1.Application.Logic.Persistence;
using Mokkit.Example1.Domain.Entities;

namespace Mokkit.Example1.Infrastructure.Logic.Cache;

/// <summary>
/// Redis-based implementation of client caching service.
/// </summary>
internal sealed class ClientCacheService : IClientCacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<ClientCacheService> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public ClientCacheService(
        IDistributedCache distributedCache,
        ILogger<ClientCacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<Client?> GetClientAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = GetCacheKey(clientId);
            var cachedData = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
            
            if (string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("Client not found in cache: {ClientId}", clientId);
                return null;
            }

            var client = JsonSerializer.Deserialize<Client>(cachedData);
            _logger.LogDebug("Client retrieved from cache: {ClientId}", clientId);
            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving client from cache: {ClientId}", clientId);
            return null; // Graceful degradation - cache miss
        }
    }

    public async Task SetClientAsync(Client client, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = GetCacheKey(client.Id);
            var serializedData = JsonSerializer.Serialize(client);
            
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            };

            await _distributedCache.SetStringAsync(cacheKey, serializedData, options, cancellationToken);
            _logger.LogDebug("Client cached: {ClientId}", client.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching client: {ClientId}", client.Id);
            // Don't throw - caching is not critical for functionality
        }
    }

    public async Task RemoveClientAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = GetCacheKey(clientId);
            await _distributedCache.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogDebug("Client removed from cache: {ClientId}", clientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing client from cache: {ClientId}", clientId);
            // Don't throw - cache removal is not critical
        }
    }

    private static string GetCacheKey(Guid clientId) => $"client:{clientId}";
}
