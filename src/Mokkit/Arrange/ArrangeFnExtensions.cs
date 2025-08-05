namespace Mokkit.Arrange;

/// <summary>
/// Provides extension methods for arrange function delegates to enable fluent chaining of arrange operations.
/// These extensions allow direct chaining of arrange functions without explicitly creating <see cref="ITestArrange"/> instances.
/// </summary>
public static class ArrangeFnExtensions
{
    /// <summary>
    /// Chains an asynchronous arrange function with another asynchronous arrange function.
    /// </summary>
    /// <param name="arrangeFn">The first asynchronous arrange function to execute.</param>
    /// <param name="thenFn">The second asynchronous arrange function to execute after the first.</param>
    /// <returns>An <see cref="ITestArrange"/> instance that will execute both functions in sequence.</returns>
    public static ITestArrange Then(this ArrangeAsyncFn arrangeFn, ArrangeAsyncFn thenFn) =>
        Arrange.Start(arrangeFn).Then(thenFn);

    /// <summary>
    /// Chains an asynchronous arrange function with a synchronous arrange function.
    /// </summary>
    /// <param name="arrangeFn">The first asynchronous arrange function to execute.</param>
    /// <param name="thenFn">The second synchronous arrange function to execute after the first.</param>
    /// <returns>An <see cref="ITestArrange"/> instance that will execute both functions in sequence.</returns>
    public static ITestArrange Then(this ArrangeAsyncFn arrangeFn, ArrangeFn thenFn) =>
        Arrange.Start(arrangeFn).Then(thenFn);

    /// <summary>
    /// Chains a synchronous arrange function with an asynchronous arrange function.
    /// </summary>
    /// <param name="arrangeFn">The first synchronous arrange function to execute.</param>
    /// <param name="thenFn">The second asynchronous arrange function to execute after the first.</param>
    /// <returns>An <see cref="ITestArrange"/> instance that will execute both functions in sequence.</returns>
    public static ITestArrange Then(this ArrangeFn arrangeFn, ArrangeAsyncFn thenFn) =>
        Arrange.Start(arrangeFn).Then(thenFn);

    /// <summary>
    /// Chains a synchronous arrange function with another synchronous arrange function.
    /// </summary>
    /// <param name="arrangeFn">The first synchronous arrange function to execute.</param>
    /// <param name="thenFn">The second synchronous arrange function to execute after the first.</param>
    /// <returns>An <see cref="ITestArrange"/> instance that will execute both functions in sequence.</returns>
    public static ITestArrange Then(this ArrangeFn arrangeFn, ArrangeFn thenFn) =>
        Arrange.Start(arrangeFn).Then(thenFn);
}