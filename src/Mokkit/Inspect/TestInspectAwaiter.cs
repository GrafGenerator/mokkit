using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Mokkit.Inspect;

/// <summary>
/// Internal implementation of <see cref="ITestInspectAwaiter"/> that provides awaitable functionality for inspect operations.
/// This class manages the asynchronous execution of inspect functions and handles synchronization context preservation.
/// </summary>
internal class TestInspectAwaiter : ITestInspectAwaiter
{
    /// <summary>
    /// The synchronization context captured at the time of instance creation, or null if no context was available.
    /// </summary>
    private readonly SynchronizationContext? _capturedContext = SynchronizationContext.Current;
    
    /// <summary>
    /// Gets a value indicating whether the asynchronous operation has completed.
    /// </summary>
    public bool IsCompleted => _action.IsCompleted;
    
    /// <summary>
    /// The asynchronous operation being executed.
    /// </summary>
    private readonly Task _action;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TestInspectAwaiter"/> class with the specified inspect instance.
    /// This constructor starts the asynchronous execution of the inspect functions.
    /// </summary>
    /// <param name="inspect">The test inspect instance containing the functions to execute.</param>
    internal TestInspectAwaiter(TestInspect inspect)
    {
        _action = inspect.DoInspectAsync();
    }
    
    /// <summary>
    /// Blocks the calling thread until the asynchronous operation completes.
    /// </summary>
    public void GetResult()
    {
        SpinWait.SpinUntil(() => IsCompleted);
        
        RethrowOnFault();
    }
    
    /// <summary>
    /// Schedules the continuation action to run on the captured synchronization context.
    /// </summary>
    /// <param name="continuation">The action to schedule for execution.</param>
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
    /// Rethrows any faults that occurred during the execution of the inspect functions.
    /// </summary>
    private void RethrowOnFault()
    {
        if (_action is { IsFaulted: true, Exception.InnerException: not null })
        {
            ExceptionDispatchInfo.Capture(_action.Exception.InnerException).Throw();
        }
    }
}