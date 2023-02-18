using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Mokkit.Capture.Suite;

namespace Mokkit.Capture;

public class TestArrange : ITestArrange, ITestArrangeProvider
{
    private readonly TestStage _stage;
    private readonly List<ArrangeAsyncFn> _arrangeFns = new();

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
    
    IReadOnlyCollection<ArrangeAsyncFn> ITestArrangeProvider.GetArrangeFunctions() => _arrangeFns;

    public ITestArrangeAwaiter GetAwaiter()
    {
        return new TestArrangeAwaiter(this);
    }
}

public interface ITestArrangeAwaiter: INotifyCompletion
{
    bool IsCompleted { get; }
    void GetResult();
}

public class TestArrangeAwaiter : ITestArrangeAwaiter
{
    private readonly SynchronizationContext? _capturedContext = SynchronizationContext.Current;
    
    public bool IsCompleted => _action.IsCompleted;
    
    private readonly Task _action;
    
    internal TestArrangeAwaiter(TestArrange arrange)
    {
        _action = arrange.DoArrangeAsync();
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