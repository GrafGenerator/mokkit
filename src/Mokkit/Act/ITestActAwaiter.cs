using System.Runtime.CompilerServices;

namespace Mokkit.Act;

/// <summary>
/// Provides an awaiter for act operations, enabling async/await support for act chains.
/// This interface implements the awaiter pattern to allow direct awaiting of <see cref="ITestAct"/> instances.
/// </summary>
public interface ITestActAwaiter : INotifyCompletion
{
    /// <summary>
    /// Gets a value indicating whether the act operation has completed.
    /// </summary>
    /// <value><c>true</c> if the act operation is complete; otherwise, <c>false</c>.</value>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets the result of the act operation. This method is called when the await completes.
    /// </summary>
    void GetResult();
}

/// <summary>
/// Provides an awaiter for result-bearing act operations, enabling <c>var r = await Act.DoThing(...)</c>.
/// This interface implements the awaiter pattern to allow direct awaiting of <see cref="ITestAct{T}"/> instances.
/// </summary>
/// <typeparam name="T">The type of the produced result.</typeparam>
public interface ITestActAwaiter<out T> : INotifyCompletion
{
    /// <summary>
    /// Gets a value indicating whether the act operation has completed.
    /// </summary>
    /// <value><c>true</c> if the act operation is complete; otherwise, <c>false</c>.</value>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets the result produced by the act operation. This method is called when the await completes.
    /// </summary>
    /// <returns>The value produced by the act.</returns>
    T GetResult();
}
