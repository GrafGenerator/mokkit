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

    private sealed record Entity(Guid Id);

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

    private static void AssertGuardThrows<T>(T value) =>
        Assert.Throws<InvalidOperationException>(() => Inspector().Ensure(value, out T _));
}
