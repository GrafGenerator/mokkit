using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mokkit.Capture;

public class TestArrange : ITestArrange, ITestArrangeProvider
{
    private readonly List<ArrangeAsyncFn> _arrangeFns = new();

    internal TestArrange(ArrangeFn arrangeFn)
    {
        _arrangeFns.Add(() =>
        {
            arrangeFn();
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
        _arrangeFns.Add(() =>
        { 
            arrangeFn();
            return Task.CompletedTask;
        });
        
        return this;
    }

    IReadOnlyCollection<ArrangeAsyncFn> ITestArrangeProvider.GetArrangeFunctions() => _arrangeFns;
}