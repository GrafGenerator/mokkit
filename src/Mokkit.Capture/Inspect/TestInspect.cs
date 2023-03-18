using System.Collections.Generic;
using System.Threading.Tasks;
using Mokkit.Capture.Suite;

namespace Mokkit.Capture.Inspect;

public class TestInspect : ITestInspect
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