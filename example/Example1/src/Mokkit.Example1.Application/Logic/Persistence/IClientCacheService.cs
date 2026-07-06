namespace Mokkit.Example1.Application.Logic.Persistence;

/// <summary>
/// Interface for client caching operations.
/// </summary>
public interface IClientCacheService
{
    /// <summary>
    /// Get client from cache by ID.
    /// </summary>
    /// <param name="clientId">Client ID to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Client if found, null otherwise</returns>
    Task<Domain.Entities.Client?> GetClientAsync(Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set client in cache.
    /// </summary>
    /// <param name="client">Client to cache</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetClientAsync(Domain.Entities.Client client, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove client from cache.
    /// </summary>
    /// <param name="clientId">Client ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveClientAsync(Guid clientId, CancellationToken cancellationToken = default);
}
