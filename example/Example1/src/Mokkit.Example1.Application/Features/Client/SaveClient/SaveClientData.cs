namespace Mokkit.Example1.Application.Features.Client.SaveClient;

public record SaveClientData
{
    public Guid? Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Phone { get; set; } = string.Empty;
    
    public int Status { get; set; } = 1; // Active by default
}
