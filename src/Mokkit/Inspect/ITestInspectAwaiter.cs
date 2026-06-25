using System.Runtime.CompilerServices;

namespace Mokkit.Inspect;

/// <summary>
/// Provides an awaiter for inspect operations, enabling async/await support for inspect chains.
/// This interface implements the awaiter pattern to allow direct awaiting of <see cref="ITestInspect"/> instances.
/// </summary>
public interface ITestInspectAwaiter: INotifyCompletion
{
    /// <summary>
    /// Gets a value indicating whether the inspect operation has completed.
    /// </summary>
    /// <value><c>true</c> if the inspect operation is complete; otherwise, <c>false</c>.</value>
    bool IsCompleted { get; }
    
    /// <summary>
    /// Gets the result of the inspect operation. This method is called when the await completes.
    /// </summary>
    void GetResult();
}