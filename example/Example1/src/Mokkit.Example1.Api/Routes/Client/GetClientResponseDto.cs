using Mokkit.Example1.Domain.Entities;

namespace Mokkit.Example1.Api.Routes.Client;

public record GetClientResponseDto
{
    public Guid Id { get; init; }
    
    public string Name { get; init; } = string.Empty;
    
    public string Email { get; init; } = string.Empty;
    
    public string Phone { get; init; } = string.Empty;
    
    public ClientStatus Status { get; init; }
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime UpdatedAt { get; init; }
}
