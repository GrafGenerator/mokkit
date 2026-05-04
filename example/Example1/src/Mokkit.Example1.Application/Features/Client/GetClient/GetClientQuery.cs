namespace Mokkit.Example1.Application.Features.Client.GetClient;

/// <summary>
/// Query to get a client by ID.
/// </summary>
public record struct GetClientQuery
{
    public Guid ClientId { get; set; }
}
