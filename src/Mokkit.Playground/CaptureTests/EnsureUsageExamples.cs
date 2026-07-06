using System;
using System.Threading.Tasks;
using Mokkit.Arrange;
using Mokkit.Inspect;

namespace Mokkit.Playground.CaptureTests;

/// <summary>
/// Compile-only examples that exercise every <c>Ensure</c> overload so overload resolution and type inference are
/// verified at build time. These methods are illustrative and are never invoked.
/// </summary>
internal static class EnsureUsageExamples
{
    private sealed record Artifact(Guid? Id, string Name);

    private static async Task ArrangeUsage(ITestArrange arrange, Trapture<Foo> foo, Capture<Foo> foo2)
    {
        await arrange
            // selector overload — source is a Trapture (ICapture<Foo>); derives a non-empty int
            .Ensure(foo, f => f.Value, out Trapture<int> _)
            // selector overload — source is a Capture (also ICapture<Foo>)
            .Ensure(foo2, f => f.Value, out Trapture<int> _)
            // thunk overload — derives across the capture explicitly
            .Ensure(() => foo.Value!.Value, out Trapture<int> _);
    }

    private static void InspectUsage(ITestInspect inspect, Artifact artifact)
    {
        inspect
            // nullable-struct selector — unwraps Guid? to Guid, no !.Value noise
            .Ensure(artifact, a => a.Id, out Guid _)
            // direct-value overload — already-materialized string
            .Ensure(artifact.Name, out string _);
    }

    private static async Task ParallelInspectUsage(ITestInspect inspect)
    {
        await inspect
            .Then(_ => { })
            // raw async parallel group — these two overlap; steps before/after stay sequential
            .ThenAll(
                async _ => await Task.CompletedTask,
                async _ => await Task.CompletedTask)
            // raw sync parallel group
            .ThenAll(
                _ => { },
                _ => { })
            // branch-builder group — each branch is its own sub-chain of fluent helpers, built with the
            // same chain style; branches overlap, steps within a branch stay sequential
            .ThenAll(
                b => b.Then(_ => { }).Then(_ => { }),
                b => b.Then(_ => { }))
            .Then(_ => { });
    }
}
