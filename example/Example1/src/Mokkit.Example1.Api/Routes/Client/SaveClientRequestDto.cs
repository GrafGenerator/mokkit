namespace Mokkit.Example1.Api.Routes.Client;

public class SaveClientRequestDto
{
    public string Name { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Phone { get; set; } = string.Empty;
    
    public int Status { get; set; } = 1; // Active by default
}
