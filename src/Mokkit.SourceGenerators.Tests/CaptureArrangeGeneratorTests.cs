using System.Threading.Tasks;
using Mokkit;
using Mokkit.Arrange;
using Mokkit.Containers.Bag;
using Mokkit.Suite;
using Xunit;

namespace Mokkit.SourceGenerators.Tests;

public sealed record Foo(int Value, string Name); // positional -> constructor

public sealed record Bar(int Code); // positional -> constructor

public sealed record Baz // init properties -> object initializer
{
    public int Code { get; init; }

    public string Label { get; init; } = string.Empty;
}

// Bodies are supplied by the source generator from the [MokkitCapture] markers below.
public static partial class GeneratedArranges
{
    [MokkitCapture]
    public static partial ITestArrange ArrangeFoo(this ITestArrange arrange, out Trapture<Foo> foo, int value, string name);

    [MokkitCapture]
    public static partial ITestArrange ArrangeBar(this ITestArrange arrange, out Capture<Bar> bar, int code);

    [MokkitCapture]
    public static partial ITestArrange ArrangeBaz(this ITestArrange arrange, out Trapture<Baz> baz, int code, string label);
}

public class CaptureArrangeGeneratorTests
{
    private static async Task<TestStage> NewStage()
    {
        var setup = await TestStageSetup.Create(new BagContainerBuilder());
        return setup.EnterStage();
    }

    [Fact]
    public async Task GeneratedArrange_Trapture_BuildsAndSetsValue()
    {
        var stage = await NewStage();

        await stage.Arrange().ArrangeFoo(out var foo, 42, "acme");

        Assert.Equal(42, foo.Value!.Value);
        Assert.Equal("acme", foo.Value!.Name);

        Foo transparent = foo; // implicit Trapture<Foo> -> Foo
        Assert.Equal("acme", transparent.Name);
    }

    [Fact]
    public async Task GeneratedArrange_Capture_BuildsAndSetsValue()
    {
        var stage = await NewStage();

        await stage.Arrange().ArrangeBar(out var bar, 7);

        Assert.NotNull(bar.Value);
        Assert.Equal(7, bar.Value!.Code);
    }

    [Fact]
    public async Task GeneratedArrange_InitPropertyRecord_UsesObjectInitializer()
    {
        var stage = await NewStage();

        await stage.Arrange().ArrangeBaz(out var baz, 9, "widget");

        Assert.Equal(9, baz.Value!.Code);
        Assert.Equal("widget", baz.Value!.Label);
    }
}
