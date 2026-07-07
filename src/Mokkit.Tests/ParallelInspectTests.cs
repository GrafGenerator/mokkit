using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mokkit.Inspect;
using Xunit;

namespace Mokkit.Tests;

public class ParallelInspectTests
{
    [Fact]
    public async Task ThenAll_RunsBranchesConcurrently()
    {
        var stage = await StageHelper.EmptyStage();
        using var barrier = new Barrier(2);
        var reached = new bool[2];

        // If the two steps ran sequentially, the first SignalAndWait would never see a second participant
        // and would time out. Passing the rendezvous proves they overlap.
        await stage.Inspect().ThenAll(
            async _ =>
            {
                await Task.Yield();
                reached[0] = barrier.SignalAndWait(TimeSpan.FromSeconds(5));
            },
            async _ =>
            {
                await Task.Yield();
                reached[1] = barrier.SignalAndWait(TimeSpan.FromSeconds(5));
            });

        Assert.True(reached[0]);
        Assert.True(reached[1]);
    }

    [Fact]
    public async Task Chain_PreservesOrder_AroundParallelGroup()
    {
        var stage = await StageHelper.EmptyStage();
        var order = new List<int>();

        void Add(int n)
        {
            lock (order)
            {
                order.Add(n);
            }
        }

        await stage.Inspect()
            .Then(_ => Add(1))
            .Then(_ => Add(2))
            .ThenAll(
                async _ =>
                {
                    await Task.Delay(40);
                    Add(3);
                },
                async _ => Add(4))
            .Then(_ => Add(5));

        Assert.Equal(1, order[0]);
        Assert.Equal(2, order[1]);
        Assert.Equal(5, order[^1]);
        Assert.Contains(3, order);
        Assert.Contains(4, order);
    }

    [Fact]
    public async Task ThenAll_BranchBuilder_RunsSubChains()
    {
        var stage = await StageHelper.EmptyStage();
        var hits = new ConcurrentBag<string>();

        await stage.Inspect().ThenAll(
            b => b.Then(_ => hits.Add("a1")).Then(_ => hits.Add("a2")),
            b => b.Then(_ => hits.Add("b1")));

        Assert.Contains("a1", hits);
        Assert.Contains("a2", hits);
        Assert.Contains("b1", hits);
    }

    [Fact]
    public async Task ThenAll_HighFanout_ResolvesSafely()
    {
        var counter = new Counter();
        var stage = await StageHelper.StageWith(b => b.AddInstance<ICounter>(counter));
        var resolved = new ConcurrentBag<ICounter>();

        var fns = Enumerable.Range(0, 64).Select<int, InspectAsyncFn>(_ => async host =>
        {
            await Task.Yield();
            host.Execute<ICounter>(c =>
            {
                resolved.Add(c);
                c.Next();
            });
        }).ToArray();

        await stage.Inspect().ThenAll(fns);

        Assert.Equal(64, resolved.Count);
        Assert.All(resolved, r => Assert.Same(counter, r)); // concurrent resolves all hit the same cached instance
    }
}
