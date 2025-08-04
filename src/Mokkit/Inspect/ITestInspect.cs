namespace Mokkit.Inspect;

/// <summary>
/// Represents the fluent interface for chaining inspect operations in the AAA (Arrange-Act-Assert) pattern.
/// This interface allows for sequential execution of assertions and verifications through method chaining.
/// </summary>
public interface ITestInspect
{
    /// <summary>
    /// Chains an asynchronous inspect function to be executed in sequence.
    /// </summary>
    /// <param name="inspectFn">The asynchronous inspect function to execute.</param>
    /// <returns>The current <see cref="ITestInspect"/> instance for method chaining.</returns>
    ITestInspect Then(InspectAsyncFn inspectFn);

    /// <summary>
    /// Chains a synchronous inspect function to be executed in sequence.
    /// </summary>
    /// <param name="inspectFn">The synchronous inspect function to execute.</param>
    /// <returns>The current <see cref="ITestInspect"/> instance for method chaining.</returns>
    ITestInspect Then(InspectFn inspectFn);

    /// <summary>
    /// Creates a value-scoped inspection that operates on a specific value.
    /// This allows for focused assertions on a particular value within a scoped context.
    /// </summary>
    /// <typeparam name="T">The type of the value to inspect.</typeparam>
    /// <param name="value">The value to inspect within the scope.</param>
    /// <param name="inspectScopeFn">Optional scoped inspect function to execute within the value scope.</param>
    /// <returns>A <see cref="ITestInspectScope{T}"/> for value-scoped operations.</returns>
    ITestInspectScope<T> ThenValueScope<T>(T value, InspectScopeAsyncFn? inspectScopeFn = null);

    /// <summary>
    /// Creates a value-scoped inspection that operates on a specific value with additional context.
    /// This allows for focused assertions on a particular value with contextual information.
    /// </summary>
    /// <typeparam name="T">The type of the value to inspect.</typeparam>
    /// <typeparam name="TContext">The type of the context object.</typeparam>
    /// <param name="value">The value to inspect within the scope.</param>
    /// <param name="context">Additional context information for the inspection.</param>
    /// <param name="inspectScopeFn">Optional scoped inspect function to execute within the value scope.</param>
    /// <returns>A <see cref="ITestInspectScopeWithContext{T, TContext}"/> for value-scoped operations with context.</returns>
    ITestInspectScopeWithContext<T, TContext> ThenValueScope<T, TContext>(T value, TContext context,
        InspectScopeAsyncFn? inspectScopeFn = null);

    /// <summary>
    /// Gets an awaiter that allows the inspect chain to be awaited using the async/await pattern.
    /// This enables the use of 'await' directly on the inspect chain.
    /// </summary>
    /// <returns>An awaiter for the inspect operation chain.</returns>
    ITestInspectAwaiter GetAwaiter();
}