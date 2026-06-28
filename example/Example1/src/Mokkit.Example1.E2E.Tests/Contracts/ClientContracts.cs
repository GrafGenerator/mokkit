namespace Mokkit.Example1.E2E.Tests.Contracts;

// Black-box wire contracts owned by the E2E suite — deliberately decoupled from the service's
// internal request/response DTOs. Status is an int because the API serialises the enum as a number.

public sealed record SaveClientRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public int Status { get; init; } = 1;
}

public sealed record SaveClientResponse
{
    public bool Success { get; init; }
    public Guid ClientId { get; init; }
    public string? Message { get; init; }
}

public sealed record ClientResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public int Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed record StatusChangedMessage
{
    public Guid ClientId { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public int Status { get; init; }
}
