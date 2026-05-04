using Mokkit.Example1.Domain.Entities;

namespace Mokkit.Example1.Application.Features.Client.GetClient;

/// <summary>
/// Result of get client query execution.
/// </summary>
public record GetClientQueryResult
{
    public bool Success { get; init; }
    
    public Domain.Entities.Client? Client { get; init; }
    
    public Exception? Exception { get; init; }

    public GetClientQueryResult(bool success, Domain.Entities.Client? client, Exception? exception = null)
    {
        Success = success;
        Client = client;
        Exception = exception;
    }
}
