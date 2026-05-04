namespace Mokkit.Example1.Api.Routes.Client;

public record SaveClientResponseDto
{
    public bool Success { get; init; }
    
    public Guid ClientId { get; init; }

    public string? Message { get; init; }
}
