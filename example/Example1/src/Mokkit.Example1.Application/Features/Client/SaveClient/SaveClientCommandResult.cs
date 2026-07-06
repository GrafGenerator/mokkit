namespace Mokkit.Example1.Application.Features.Client.SaveClient;

/// <summary>
/// Result of save client command execution.
/// </summary>
public record SaveClientCommandResult
{
    public bool Success { get; init; }
    
    public Guid? ClientId { get; init; }
    
    public Exception? Exception { get; init; }

    public SaveClientCommandResult(bool success, Guid? clientId, Exception? exception = null)
    {
        Success = success;
        ClientId = clientId;
        Exception = exception;
    }
}
