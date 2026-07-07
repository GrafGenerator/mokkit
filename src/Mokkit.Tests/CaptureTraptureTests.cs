using System;
using Xunit;

namespace Mokkit.Tests;

public class CaptureTraptureTests
{
    [Fact]
    public void Capture_Value_IsDefault_WhenUnset()
    {
        Capture.Start(out Capture<string> capture);

        Assert.Null(capture.Value);
    }

    [Fact]
    public void Capture_Set_ExposesValue()
    {
        var initializer = Capture.Start(out Capture<string> capture);

        initializer.Set("hello");

        Assert.Equal("hello", capture.Value);
    }

    [Fact]
    public void Trapture_ImplicitConversion_ReturnsValue()
    {
        var initializer = Trapture.Start(out Trapture<int> trapture);

        initializer.Set(42);

        int value = trapture;

        Assert.Equal(42, value);
    }

    [Fact]
    public void Trapture_ImplicitConversion_Throws_WhenUnset()
    {
        Trapture.Start(out Trapture<string> trapture);

        Assert.Throws<InvalidOperationException>(() => Consume(trapture));
    }

    // Forces the implicit Trapture<string> -> string conversion at the call site.
    private static string Consume(string value) => value;
}
