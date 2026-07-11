using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mokkit.Act;
using Mokkit.Suite;
using Xunit;

namespace Mokkit.Tests;

public class ActTests
{
    // --- Void flavor: an act just performs the operation under test ---

    [Fact]
    public async Task VoidAct_RunsWhenAwaited()
    {
        var counter = new Counter();
        var stage = await StageHelper.StageWith(b => b.AddInstance<ICounter>(counter));

        await stage.Act().Then(host => host.Execute<ICounter>(c => c.Next()));

        Assert.Equal(1, counter.Value);
    }

    [Fact]
    public async Task VoidAct_RunsChainedStepsInOrder()
    {
        var stage = await StageHelper.EmptyStage();
        var order = new List<int>();

        await stage.Act()
            .Then(_ => order.Add(1))
            .Then(async _ =>
            {
                await Task.Yield();
                order.Add(2);
            })
            .Then(_ => order.Add(3));

        Assert.Equal(new[] { 1, 2, 3 }, order);
    }

    // --- Capture flavor: the act's result threads forward through an out Trapture ---

    [Fact]
    public async Task CaptureAct_SetsValue_AfterAwait()
    {
        var stage = await StageHelper.StageWith(b => b.AddInstance<ICounter>(new Counter()));

        // Mirrors the "capture" vocabulary flavor: the operation sets an out capture when the chain runs.
        var initializer = Trapture.Start(out Trapture<int> next);
        await stage.Act().Then(host => host.Execute<ICounter>(c => initializer.Set(c.Next())));

        Assert.Equal(1, next.Value);
        Assert.Equal(1, (int)next); // Trapture converts transparently for later steps
    }

    // --- Return flavor: the act operation returns its artifact directly ---

    [Fact]
    public async Task ReturnAct_YieldsProducedValue()
    {
        var stage = await StageHelper.StageWith(b => b.AddInstance<ICounter>(new Counter()));

        var result = await stage.Act().Returning(host => host.Execute<ICounter, int>(c => c.Next()));

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task ReturnAct_Async_YieldsProducedValue()
    {
        var stage = await StageHelper.StageWith(b => b.AddInstance<ICounter>(new Counter()));

        var result = await stage.Act().Returning(async host =>
        {
            await Task.Yield();
            return host.Execute<ICounter, int>(c => c.Next());
        });

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task ReturnAct_RunsPriorSteps_ThenProducer()
    {
        var counter = new Counter();
        var stage = await StageHelper.StageWith(b => b.AddInstance<ICounter>(counter));

        // Two prior steps advance the counter, then the producer reads the next value.
        var result = await stage.Act()
            .Then(host => host.Execute<ICounter>(c => c.Next()))
            .Then(host => host.Execute<ICounter>(c => c.Next()))
            .Returning(host => host.Execute<ICounter, int>(c => c.Next()));

        Assert.Equal(3, result);
        Assert.Equal(3, counter.Value);
    }

    // --- Exceptions propagate through the awaiter, like Arrange/Inspect ---

    [Fact]
    public async Task VoidAct_PropagatesException()
    {
        var stage = await StageHelper.EmptyStage();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await stage.Act().Then(_ => throw new InvalidOperationException("boom")));
    }

    [Fact]
    public async Task ReturnAct_PropagatesException()
    {
        var stage = await StageHelper.EmptyStage();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await stage.Act().Returning((Func<ITestHost, int>)(_ => throw new InvalidOperationException("boom"))));
    }
}
