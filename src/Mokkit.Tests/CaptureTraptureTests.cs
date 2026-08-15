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

    // --- EnsureValue: read the whole value without the .Value! dance ---

    [Fact]
    public void Capture_EnsureValue_ReturnsValue()
    {
        var initializer = Capture.Start(out Capture<Entity> capture);
        var entity = new Entity(Guid.NewGuid(), "Acme");

        initializer.Set(entity);

        Assert.Same(entity, capture.EnsureValue);
    }

    [Fact]
    public void Capture_EnsureValue_Throws_WhenUnset()
    {
        Capture.Start(out Capture<Entity> capture);

        Assert.Throws<InvalidOperationException>(() => capture.EnsureValue);
    }

    [Fact]
    public void Trapture_EnsureValue_Throws_WhenUnset()
    {
        Trapture.Start(out Trapture<Entity> trapture);

        Assert.Throws<InvalidOperationException>(() => trapture.EnsureValue);
    }

    // EnsureValue carries the Ensure family's notion of "empty", which is what lets it catch an unfilled
    // value-type capture — there is no null to check, but Guid.Empty is still empty.
    [Fact]
    public void EnsureValue_Throws_OnUnfilledValueTypeCapture()
    {
        Capture.Start(out Capture<Guid> capture);

        Assert.Throws<InvalidOperationException>(() => capture.EnsureValue);
    }

    [Fact]
    public void EnsureValue_Throws_OnEmptyString()
    {
        var initializer = Capture.Start(out Capture<string> capture);
        initializer.Set(string.Empty);

        Assert.Throws<InvalidOperationException>(() => capture.EnsureValue);
    }

    [Fact]
    public void EnsureValue_IsAvailableOnICapture()
    {
        var initializer = Capture.Start(out Capture<Entity> capture);
        var entity = new Entity(Guid.NewGuid(), "Acme");
        initializer.Set(entity);

        ICapture<Entity> readOnly = capture;

        Assert.Same(entity, readOnly.EnsureValue);
    }

    // --- Prop: read a member without the .Value! dance ---

    [Fact]
    public void Capture_Prop_ReadsMember()
    {
        var initializer = Capture.Start(out Capture<Entity> capture);
        var id = Guid.NewGuid();

        initializer.Set(new Entity(id, "Acme"));

        Assert.Equal(id, capture.Prop(e => e.Id));
        Assert.Equal("Acme", capture.Prop(e => e.Name));
    }

    [Fact]
    public void Capture_Prop_Throws_WhenUnset()
    {
        Capture.Start(out Capture<Entity> capture);

        Assert.Throws<InvalidOperationException>(() => capture.Prop(e => e.Id));
    }

    [Fact]
    public void Trapture_Prop_ReadsMember()
    {
        var initializer = Trapture.Start(out Trapture<Entity> trapture);
        var id = Guid.NewGuid();

        initializer.Set(new Entity(id, "Acme"));

        Assert.Equal(id, trapture.Prop(e => e.Id));
    }

    [Fact]
    public void Trapture_Prop_Throws_WhenUnset()
    {
        Trapture.Start(out Trapture<Entity> trapture);

        Assert.Throws<InvalidOperationException>(() => trapture.Prop(e => e.Id));
    }

    [Fact]
    public void Prop_Throws_OnNullSelector()
    {
        var initializer = Capture.Start(out Capture<Entity> capture);
        initializer.Set(new Entity(Guid.NewGuid(), "Acme"));

        Assert.Throws<ArgumentNullException>(() => capture.Prop<string>(null!));
    }

    // Prop guards the capture, not the member — a nullable member still comes back null.
    [Fact]
    public void Prop_ReturnsNull_WhenMemberIsNull()
    {
        var initializer = Capture.Start(out Capture<Entity> capture);
        initializer.Set(new Entity(Guid.NewGuid(), "Acme", Note: null));

        Assert.Null(capture.Prop(e => e.Note));
    }

    // Prop is EnsureValue + a selector, so it inherits the same guard.
    [Fact]
    public void Prop_Throws_OnUnfilledValueTypeCapture()
    {
        Capture.Start(out Capture<Guid> capture);

        Assert.Throws<InvalidOperationException>(() => capture.Prop(g => g.ToString()));
    }

    // Prop is reachable through the covariant read-only contract, not just the concrete types.
    [Fact]
    public void Prop_IsAvailableOnICapture()
    {
        var initializer = Capture.Start(out Capture<Entity> capture);
        var id = Guid.NewGuid();
        initializer.Set(new Entity(id, "Acme"));

        ICapture<Entity> readOnly = capture;

        Assert.Equal(id, readOnly.Prop(e => e.Id));
    }

    // Forces the implicit Trapture<string> -> string conversion at the call site.
    private static string Consume(string value) => value;

    private sealed record Entity(Guid Id, string Name, string? Note = null);
}
