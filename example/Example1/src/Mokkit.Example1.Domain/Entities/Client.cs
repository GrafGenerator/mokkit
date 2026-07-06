namespace Mokkit.Example1.Domain.Entities;

/// <summary>
/// Client entity model.
/// </summary>
public sealed class Client
{
    /// <summary>
    /// Client ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Client full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Client email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Client phone number.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Client status.
    /// </summary>
    public ClientStatus Status { get; set; } = ClientStatus.Active;

    /// <summary>
    /// Date when client was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date when client was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Client status enumeration.
/// </summary>
public enum ClientStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3
}