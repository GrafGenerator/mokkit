namespace Mokkit.Inspect;

/// <summary>
/// Represents a value-scoped inspect interface that operates on a specific value of type <typeparamref name="T"/>.
/// This interface extends <see cref="ITestInspect"/> to provide value-specific inspection operations within a scoped context.
/// </summary>
/// <typeparam name="T">The type of value being inspected within this scope.</typeparam>
public interface ITestInspectScope<out T>: ITestInspect
{
    /// <summary>
    /// Chains an asynchronous value-specific inspect function to be executed in sequence.
    /// The function will receive the scoped value for inspection.
    /// </summary>
    /// <param name="inspectFn">The asynchronous inspect function that operates on the scoped value.</param>
    /// <returns>The current <see cref="ITestInspectScope{T}"/> instance for method chaining.</returns>
    ITestInspectScope<T> Then(InspectValueAsyncFn<T> inspectFn);

    /// <summary>
    /// Chains a synchronous value-specific inspect function to be executed in sequence.
    /// The function will receive the scoped value for inspection.
    /// </summary>
    /// <param name="inspectFn">The synchronous inspect function that operates on the scoped value.</param>
    /// <returns>The current <see cref="ITestInspectScope{T}"/> instance for method chaining.</returns>
    ITestInspectScope<T> Then(InspectValueFn<T> inspectFn);
}