namespace Mokkit.Playground.CaptureTests;

public class Bar
{
    public Foo Foo { get; }

    public Bar(Foo foo)
    {
        Foo = foo;
    }

    public int GetValue() => Foo.Value;
}