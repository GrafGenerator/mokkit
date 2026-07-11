using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Mokkit.Act;

/// <summary>
/// Internal implementation of <see cref="ITestActAwaiter"/> that provides awaitable functionality for act operations.
/// This class manages the asynchronous execution of act functions and handles synchronization context preservation.
/// </summary>
internal class TestActAwaiter : ITestActAwaiter
{
    /// <summary>
    /// The synchronization context captured at the time of instance creation, used for preserving the context when invoking continuations.
    /// </summary>
    private readonly SynchronizationContext? _capturedContext = SynchronizationContext.Current;

    /// <summary>
    /// Gets a value indicating whether the asynchronous act operation has completed.
    /// </summary>
    public bool IsCompleted => _action.IsCompleted;

    /// <summary>
    /// The task representing the asynchronous act operation.
    /// </summary>
    private readonly Task _action;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestActAwaiter"/> class with the specified act instance.
    /// This constructor starts the asynchronous execution of the act functions.
    /// </summary>
    /// <param name="act">The test act instance containing the functions to execute.</param>
    internal TestActAwaiter(TestAct act)
    {
        _action = act.DoActAsync();
    }

    /// <summary>
    /// Blocks the calling thread until the asynchronous act operation completes.
    /// </summary>
    public void GetResult()
    {
        SpinWait.SpinUntil(() => IsCompleted);

        RethrowOnFault();
    }

    /// <summary>
    /// Schedules the specified continuation to be invoked when the asynchronous act operation completes.
    /// </summary>
    /// <param name="continuation">The action to be invoked when the act operation completes.</param>
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
    /// Rethrows any exception that occurred during the act operation while preserving the original stack trace.
    /// This method is called internally to ensure exceptions from act functions are properly propagated.
    /// </summary>
    private void RethrowOnFault()
    {
        if (_action is { IsFaulted: true, Exception.InnerException: not null })
        {
            ExceptionDispatchInfo.Capture(_action.Exception.InnerException).Throw();
        }
    }
}

/// <summary>
/// Internal implementation of <see cref="ITestActAwaiter{T}"/> that provides awaitable functionality for
/// result-bearing act operations, yielding the produced value when the await completes.
/// </summary>
/// <typeparam name="T">The type of the produced result.</typeparam>
internal class TestActAwaiter<T> : ITestActAwaiter<T>
{
    /// <summary>
    /// The synchronization context captured at the time of instance creation, used for preserving the context when invoking continuations.
    /// </summary>
    private readonly SynchronizationContext? _capturedContext = SynchronizationContext.Current;

    /// <summary>
    /// Gets a value indicating whether the asynchronous act operation has completed.
    /// </summary>
    public bool IsCompleted => _action.IsCompleted;

    /// <summary>
    /// The task representing the asynchronous, result-bearing act operation.
    /// </summary>
    private readonly Task<T> _action;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestActAwaiter{T}"/> class with the specified act instance.
    /// This constructor starts the asynchronous execution of the act functions.
    /// </summary>
    /// <param name="act">The result-bearing act instance containing the functions to execute.</param>
    internal TestActAwaiter(TestAct<T> act)
    {
        _action = act.DoActAsync();
    }

    /// <summary>
    /// Blocks the calling thread until the asynchronous act operation completes and returns its result.
    /// </summary>
    /// <returns>The value produced by the act operation.</returns>
    public T GetResult()
    {
        SpinWait.SpinUntil(() => IsCompleted);

        RethrowOnFault();

        return _action.Result;
    }

    /// <summary>
    /// Schedules the specified continuation to be invoked when the asynchronous act operation completes.
    /// </summary>
    /// <param name="continuation">The action to be invoked when the act operation completes.</param>
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
    /// Rethrows any exception that occurred during the act operation while preserving the original stack trace.
    /// This method is called internally to ensure exceptions from act functions are properly propagated.
    /// </summary>
    private void RethrowOnFault()
    {
        if (_action is { IsFaulted: true, Exception.InnerException: not null })
        {
            ExceptionDispatchInfo.Capture(_action.Exception.InnerException).Throw();
        }
    }
}
