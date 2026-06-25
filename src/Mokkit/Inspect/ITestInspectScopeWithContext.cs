namespace Mokkit.Inspect;

/// <summary>
/// Represents a value-scoped inspect interface that operates on a specific value of type <typeparamref name="T"/> with additional context of type <typeparamref name="TContext"/>.
/// This interface extends <see cref="ITestInspect"/> to provide value-specific inspection operations with contextual information within a scoped context.
/// </summary>
/// <typeparam name="T">The type of value being inspected within this scope.</typeparam>
/// <typeparam name="TContext">The type of context object that provides additional information for the inspection.</typeparam>
public interface ITestInspectScopeWithContext<out T, out TContext>: ITestInspect
{
    /// <summary>
    /// Chains an asynchronous value-specific inspect function with context to be executed in sequence.
    /// The function will receive both the scoped value and context for inspection.
    /// </summary>
    /// <param name="inspectFn">The asynchronous inspect function that operates on the scoped value and context.</param>
    /// <returns>The current <see cref="ITestInspectScopeWithContext{T, TContext}"/> instance for method chaining.</returns>
    ITestInspectScopeWithContext<T, TContext> Then(InspectValueWithContextAsyncFn<T, TContext> inspectFn);

    /// <summary>
    /// Chains a synchronous value-specific inspect function with context to be executed in sequence.
    /// The function will receive both the scoped value and context for inspection.
    /// </summary>
    /// <param name="inspectFn">The synchronous inspect function that operates on the scoped value and context.</param>
    /// <returns>The current <see cref="ITestInspectScopeWithContext{T, TContext}"/> instance for method chaining.</returns>
    ITestInspectScopeWithContext<T, TContext> Then(InspectValueWithContextFn<T, TContext> inspectFn);
}