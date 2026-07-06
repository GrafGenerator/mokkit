using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Mokkit.Tests;

public class BagContainerTests
{
    [Fact]
    public async Task Resolve_ReturnsRegisteredInstance_AndCachesIt()
    {
        var counter = new Counter();
        var stage = await StageHelper.StageWith(b => b.AddInstance<ICounter>(counter));

        var first = 0;
        var second = 0;
        stage.Execute<ICounter>(c => first = c.Next());
        stage.Execute<ICounter>(c => second = c.Next());

        Assert.Equal(1, first);
        Assert.Equal(2, second); // same cached instance across resolves
    }

    [Fact]
    public async Task Resolve_MissingType_Throws()
    {
        var stage = await StageHelper.EmptyStage();

        Assert.Throws<InvalidOperationException>(() => stage.Execute<ICounter>(_ => { }));
    }

    [Fact]
    public async Task Factory_ProducesOnePerScope_AndDisposesOnScopeDispose()
    {
        var created = new List<Tracked>();
        var stage = await StageHelper.StageWith(b => b.AddFactory(() =>
        {
            var tracked = new Tracked();
            created.Add(tracked);
            return tracked;
        }));

        stage.Execute<Tracked>(_ => { });
        stage.Execute<Tracked>(_ => { });

        Assert.Single(created); // one instance for the whole scope
        Assert.False(created[0].Disposed);

        stage.Dispose();

        Assert.True(created[0].Disposed);
    }

    [Fact]
    public async Task Singleton_Instance_IsNotDisposed_OnScopeDispose()
    {
        var tracked = new Tracked();
        var stage = await StageHelper.StageWith(b => b.AddInstance(tracked));

        stage.Execute<Tracked>(_ => { });
        stage.Dispose();

        Assert.False(tracked.Disposed); // caller owns pre-built instances
    }
}
