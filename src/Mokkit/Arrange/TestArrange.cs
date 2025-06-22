using System.Collections.Generic;
using System.Threading.Tasks;
using Mokkit.Suite;

namespace Mokkit.Arrange;

public class TestArrange : ITestArrange
{
    private readonly TestStage _stage;
    private readonly List<ArrangeAsyncFn> _arrangeFns = [];

    internal TestArrange(TestStage stage)
    {
        _stage = stage;
    }

    internal TestArrange(ArrangeFn arrangeFn)
    {
        _arrangeFns.Add(host =>
        {
            arrangeFn(host);
            return Task.CompletedTask;
        });
    }

    internal TestArrange(ArrangeAsyncFn arrangeFn)
    {
        _arrangeFns.Add(arrangeFn);
    }

    public ITestArrange Then(ArrangeAsyncFn arrangeFn)
    {
        _arrangeFns.Add(arrangeFn);
        return this;
    }

    public ITestArrange Then(ArrangeFn arrangeFn)
    {
        _arrangeFns.Add(host =>
        {
            arrangeFn(host);
            return Task.CompletedTask;
        });

        return this;
    }

    internal async Task DoArrangeAsync()
    {
        foreach (var arrangeFn in _arrangeFns)
        {
            await arrangeFn(_stage);
        }
    }

    public ITestArrangeAwaiter GetAwaiter()
    {
        return new TestArrangeAwaiter(this);
    }
}