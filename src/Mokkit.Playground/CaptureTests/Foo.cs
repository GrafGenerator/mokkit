namespace Mokkit.Playground.CaptureTests;

public class Foo
{
    public int Value { get; }

    public string? StringValue { get; }

    public Foo(int value, string? stringValue = null)
    {
        Value = value;
        StringValue = stringValue;
    }
}