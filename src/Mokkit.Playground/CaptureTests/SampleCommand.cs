namespace Mokkit.Playground.CaptureTests;

public record SampleCommand
{
    public bool Success { get; set; }

    public int Code { get; set; }

    public string Value { get; set; }
}