using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mokkit.Capture.Inspect;

internal class TestInspectAwaiter : ITestInspectAwaiter
{
    private readonly SynchronizationContext? _capturedContext = SynchronizationContext.Current;
    
    public bool IsCompleted => _action.IsCompleted;
    
    private readonly Task _action;
    
    internal TestInspectAwaiter(TestInspect inspect)
    {
        _action = inspect.DoInspectAsync();
    }
    
    public void GetResult()
    {
        SpinWait.SpinUntil(() => IsCompleted);
    }
    
    public void OnCompleted(Action continuation)
    {
        if (_capturedContext != null)
        {
            _capturedContext.Post(_ => continuation(), null);
        }
        else
        {
            continuation();
        }
        
        // new Task(continuation).Start();
        
        // if (IsCompleted)
        // {
        //     continuation();
        // }
        // else
        // {
        //     _action.ContinueWith(task => new Task(continuation));
        //     // _continuation = continuation;
        // }
    }
}