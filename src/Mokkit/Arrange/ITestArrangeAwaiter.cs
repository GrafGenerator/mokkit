using System.Runtime.CompilerServices;

namespace Mokkit.Arrange;

/// <summary>
/// Provides an awaiter for arrange operations, enabling async/await support for arrange chains.
/// This interface implements the awaiter pattern to allow direct awaiting of <see cref="ITestArrange"/> instances.
/// </summary>
public interface ITestArrangeAwaiter : INotifyCompletion
{
    /// <summary>
    /// Gets a value indicating whether the arrange operation has completed.
    /// </summary>
    /// <value><c>true</c> if the arrange operation is complete; otherwise, <c>false</c>.</value>
    bool IsCompleted { get; }
    
    /// <summary>
    /// Gets the result of the arrange operation. This method is called when the await completes.
    /// </summary>
    void GetResult();
}