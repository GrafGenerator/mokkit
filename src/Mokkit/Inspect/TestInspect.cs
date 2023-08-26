using System.Collections.Generic;
using System.Threading.Tasks;
using Mokkit.Suite;

namespace Mokkit.Inspect;

internal class TestInspect : ITestInspect
{
    private readonly TestStage _stage;
    private readonly List<InspectAsyncFn> _inspectFns = new();

    internal TestInspect(TestStage stage)
    {
        _stage = stage;
    }
    
    internal TestInspect(InspectFn inspectFn)
    {
        _inspectFns.Add(host =>
        {
            inspectFn(host);
            return Task.CompletedTask;
        });
    }

    internal TestInspect(InspectAsyncFn inspectFn)
    {
        _inspectFns.Add(inspectFn);
    }

    public ITestInspect Then(InspectAsyncFn inspectFn)
    {
        _inspectFns.Add(inspectFn);
        return this;
    }
    
    public ITestInspect Then(InspectFn inspectFn)
    {
        _inspectFns.Add(host =>
        { 
            inspectFn(host);
            return Task.CompletedTask;
        });
        
        return this;
    }

    public ITestInspectScope<T> ThenValueScope<T>(T value, InspectScopeAsyncFn? inspectScopeFn = null)
    {
        var innerFns = new List<InspectValueAsyncFn<T>>();
        
        var scopeFn = inspectScopeFn ?? (async (_, executeInnerFns) =>
        {
            await executeInnerFns();
        });
        
        _inspectFns.Add(host => scopeFn(host, ExecuteInnerFns));
        
        return new TestInspectScope<T>(innerFns, this);

        async Task ExecuteInnerFns()
        {
            foreach (var innerFn in innerFns)
            {
                await innerFn(value, _stage);
            }
        }
    }
    
    internal async Task DoInspectAsync()
    {
        foreach (var inspectFn in _inspectFns)
        {
            await inspectFn(_stage);
        }
    }
    
    public ITestInspectAwaiter GetAwaiter()
    {
        return new TestInspectAwaiter(this);
    }
}