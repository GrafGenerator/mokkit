using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Mokkit.Arrange;

/// <summary>
/// Internal implementation of <see cref="ITestArrangeAwaiter"/> that provides awaitable functionality for arrange operations.
/// This class manages the asynchronous execution of arrange functions and handles synchronization context preservation.
/// </summary>
internal class TestArrangeAwaiter : ITestArrangeAwaiter
{
    /// <summary>
    /// The synchronization context captured at the time of instance creation, used for preserving the context when invoking continuations.
    /// </summary>
    private readonly SynchronizationContext? _capturedContext = SynchronizationContext.Current;

    /// <summary>
    /// Gets a value indicating whether the asynchronous arrange operation has completed.
    /// </summary>
    public bool IsCompleted => _action.IsCompleted;

    /// <summary>
    /// The task representing the asynchronous arrange operation.
    /// </summary>
    private readonly Task _action;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestArrangeAwaiter"/> class with the specified arrange instance.
    /// This constructor starts the asynchronous execution of the arrange functions.
    /// </summary>
    /// <param name="arrange">The test arrange instance containing the functions to execute.</param>
    internal TestArrangeAwaiter(TestArrange arrange)
    {
        _action = arrange.DoArrangeAsync();
    }

    /// <summary>
    /// Blocks the calling thread until the asynchronous arrange operation completes.
    /// </summary>
    public void GetResult()
    {
        SpinWait.SpinUntil(() => IsCompleted);

        RethrowOnFault();
    }

    /// <summary>
    /// Schedules the specified continuation to be invoked when the asynchronous arrange operation completes.
    /// </summary>
    /// <param name="continuation">The action to be invoked when the arrange operation completes.</param>
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

    /// <summary>
    /// Rethrows any exception that occurred during the arrange operation while preserving the original stack trace.
    /// This method is called internally to ensure exceptions from arrange functions are properly propagated.
    /// </summary>
    private void RethrowOnFault()
    {
        if (_action is { IsFaulted: true, Exception.InnerException: not null })
        {
            ExceptionDispatchInfo.Capture(_action.Exception.InnerException).Throw();
        }
    }
}