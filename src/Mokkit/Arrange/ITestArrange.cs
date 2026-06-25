namespace Mokkit.Arrange;

/// <summary>
/// Represents the fluent interface for chaining arrange operations in the AAA (Arrange-Act-Assert) pattern.
/// This interface allows for sequential setup of test dependencies and state through method chaining.
/// </summary>
public interface ITestArrange
{
    /// <summary>
    /// Chains an asynchronous arrange function to be executed in sequence.
    /// </summary>
    /// <param name="arrangeFn">The asynchronous arrange function to execute.</param>
    /// <returns>The current <see cref="ITestArrange"/> instance for method chaining.</returns>
    ITestArrange Then(ArrangeAsyncFn arrangeFn);
    
    /// <summary>
    /// Chains a synchronous arrange function to be executed in sequence.
    /// </summary>
    /// <param name="arrangeFn">The synchronous arrange function to execute.</param>
    /// <returns>The current <see cref="ITestArrange"/> instance for method chaining.</returns>
    ITestArrange Then(ArrangeFn arrangeFn);
    
    /// <summary>
    /// Gets an awaiter that allows the arrange chain to be awaited using the async/await pattern.
    /// This enables the use of 'await' directly on the arrange chain.
    /// </summary>
    /// <returns>An awaiter for the arrange operation chain.</returns>
    ITestArrangeAwaiter GetAwaiter();
}