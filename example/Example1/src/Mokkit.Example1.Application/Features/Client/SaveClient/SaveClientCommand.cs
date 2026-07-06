namespace Mokkit.Example1.Application.Features.Client.SaveClient;

/// <summary>
/// Save client command (create new or update existing).
/// </summary>
public record struct SaveClientCommand
{
    public SaveOperationKind Operation { get; set; } 
    
    public SaveClientData ClientData { get; set; }
}

public enum SaveOperationKind
{
    Create = 1,
    Update = 2
}
