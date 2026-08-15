using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mokkit.Arrange;
using Mokkit.Inspect;
using Xunit;

namespace Mokkit.Tests;

public class EnsureTests
{
    private sealed record Holder(Guid? Id);

    private sealed record Entity(Guid Id, string Name = "Acme");

    // A stage-free inspect chain — the eager Ensure overloads never touch the stage.
    // Fully qualified because 'Inspect' also names the Mokkit.Inspect namespace.
    private static ITestInspect Inspector() => Mokkit.Inspect.Inspect.Start((InspectFn)(_ => { }));

    // --- Inspect: eager guard, captures synchronously ---

    [Fact]
    public void InspectEnsure_DirectValue_CapturesNonEmpty()
    {
        Inspector().Ensure(Guid.NewGuid(), out Guid captured);

        Assert.NotEqual(Guid.Empty, captured);
    }

    [Fact]
    public void InspectEnsure_Throws_OnGuidEmpty() => AssertGuardThrows(Guid.Empty);

    [Fact]
    public void InspectEnsure_Throws_OnEmptyString() => AssertGuardThrows(string.Empty);

    [Fact]
    public void InspectEnsure_Throws_OnNullString() => AssertGuardThrows<string>(null!);

    [Fact]
    public void InspectEnsure_Throws_OnZeroInt() => AssertGuardThrows(0);

    [Fact]
    public void InspectEnsure_Throws_OnEmptyCollection() => AssertGuardThrows(new List<int>());

    [Fact]
    public void InspectEnsure_Passes_OnNonEmptyValues()
    {
        Inspector().Ensure("x", out string s);
        Inspector().Ensure(5, out int n);
        Inspector().Ensure(new List<int> { 1 }, out List<int> list);

        Assert.Equal("x", s);
        Assert.Equal(5, n);
        Assert.Single(list);
    }

    [Fact]
    public void InspectEnsure_Selector_UnwrapsNullableStruct()
    {
        var id = Guid.NewGuid();

        Inspector().Ensure(new Holder(id), h => h.Id, out Guid captured);

        Assert.Equal(id, captured);
    }

    [Fact]
    public void InspectEnsure_Selector_Throws_WhenNull()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Inspector().Ensure(new Holder(null), h => h.Id, out Guid _));
    }

    // --- Arrange: deferred, sets a Trapture consumed after the await ---

    [Fact]
    public async Task ArrangeEnsure_Selector_SetsTrapture_AfterAwait()
    {
        var stage = await StageHelper.EmptyStage();
        var sourceInitializer = Trapture.Start(out Trapture<Entity> source);
        var id = Guid.NewGuid();

        await stage.Arrange()
            .Then(_ => sourceInitializer.Set(new Entity(id))) // source is populated only when the chain runs
            .Ensure(source, e => e.Id, out var captured);

        Assert.Equal(id, (Guid)captured); // implicit Trapture<Guid> -> Guid
    }

    [Fact]
    public async Task ArrangeEnsure_Thunk_SetsTrapture()
    {
        var stage = await StageHelper.EmptyStage();
        var value = Guid.NewGuid();

        await stage.Arrange().Ensure(() => value, out var captured);

        Assert.Equal(value, (Guid)captured);
    }

    [Fact]
    public async Task ArrangeEnsure_Throws_OnEmpty()
    {
        var stage = await StageHelper.EmptyStage();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await stage.Arrange().Ensure(() => Guid.Empty, out Trapture<Guid> _));
    }

    // --- Inspect: reference-type selector (sibling of the nullable-struct overload) ---

    [Fact]
    public void InspectEnsure_ReferenceSelector_Captures()
    {
        Inspector().Ensure(new Entity(Guid.NewGuid(), "Acme"), e => e.Name, out string name);

        Assert.Equal("Acme", name);
    }

    [Fact]
    public void InspectEnsure_ReferenceSelector_Throws_WhenNull()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Inspector().Ensure(new Holder(null), _ => (string?)null, out string _));
    }

    [Fact]
    public void InspectEnsure_ReferenceSelector_Throws_WhenEmpty()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Inspector().Ensure(new Entity(Guid.NewGuid(), string.Empty), e => e.Name, out string _));
    }

    // --- Inspect: capture-shaped source ---

    [Fact]
    public void InspectEnsure_CaptureSource_CapturesValueMember()
    {
        var initializer = Capture.Start(out Capture<Entity> source);
        var id = Guid.NewGuid();
        initializer.Set(new Entity(id));

        Inspector().Ensure(source, e => e.Id, out Guid captured);

        Assert.Equal(id, captured);
    }

    [Fact]
    public void InspectEnsure_CaptureSource_CapturesReferenceMember()
    {
        var initializer = Capture.Start(out Capture<Entity> source);
        initializer.Set(new Entity(Guid.NewGuid(), "Acme"));

        Inspector().Ensure(source, e => e.Name, out string captured);

        Assert.Equal("Acme", captured);
    }

    [Fact]
    public void InspectEnsure_CaptureSource_Throws_WhenCaptureUnset()
    {
        Capture.Start(out Capture<Entity> source);

        Assert.Throws<InvalidOperationException>(() =>
            Inspector().Ensure(source, e => e.Id, out Guid _));
    }

    [Fact]
    public void InspectEnsure_CaptureSource_Throws_WhenMemberEmpty()
    {
        var initializer = Capture.Start(out Capture<Entity> source);
        initializer.Set(new Entity(Guid.Empty));

        Assert.Throws<InvalidOperationException>(() =>
            Inspector().Ensure(source, e => e.Id, out Guid _));
    }

    // --- Arrange: the source capture no longer has to be a reference type ---

    [Fact]
    public async Task ArrangeEnsure_Selector_AcceptsValueTypeSource()
    {
        var stage = await StageHelper.EmptyStage();
        var sourceInitializer = Trapture.Start(out Trapture<Guid> source);
        var id = Guid.NewGuid();

        await stage.Arrange()
            .Then(_ => sourceInitializer.Set(id))
            .Ensure(source, g => g.ToString(), out var captured);

        Assert.Equal(id.ToString(), (string)captured);
    }

    private static void AssertGuardThrows<T>(T value) =>
        Assert.Throws<InvalidOperationException>(() => Inspector().Ensure(value, out T _));
}
