using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Mokkit.Arrange;

internal class TestArrangeAwaiter : ITestArrangeAwaiter
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

        RethrowOnFault();
    }

    public void OnCompleted(Action continuation)
    {
        RethrowOnFault();

        if (_capturedContext != null)
        {
            _capturedContext.Post(_ => continuation(), null);
        }
        else
        {
            continuation();
        }
    }

    private void RethrowOnFault()
    {
        if (_action is { IsFaulted: true, Exception.InnerException: not null })
        {
            ExceptionDispatchInfo.Capture(_action.Exception.InnerException).Throw();
        }
    }
}