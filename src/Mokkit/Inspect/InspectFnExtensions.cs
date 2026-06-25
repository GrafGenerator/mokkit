namespace Mokkit.Inspect;

/// <summary>
/// Provides extension methods for inspect function delegates to enable fluent chaining of inspect operations.
/// These extensions allow direct chaining of inspect functions without explicitly creating <see cref="ITestInspect"/> instances.
/// </summary>
public static class InspectFnExtensions
{
    /// <summary>
    /// Chains an asynchronous inspect function with another asynchronous inspect function.
    /// </summary>
    /// <param name="inspectFn">The first asynchronous inspect function to execute.</param>
    /// <param name="thenFn">The second asynchronous inspect function to execute after the first.</param>
    /// <returns>An <see cref="ITestInspect"/> instance that will execute both functions in sequence.</returns>
    public static ITestInspect Then(this InspectAsyncFn inspectFn, InspectAsyncFn thenFn) =>
        Inspect.Start(inspectFn).Then(thenFn);
    
    /// <summary>
    /// Chains an asynchronous inspect function with a synchronous inspect function.
    /// </summary>
    /// <param name="inspectFn">The first asynchronous inspect function to execute.</param>
    /// <param name="thenFn">The second synchronous inspect function to execute after the first.</param>
    /// <returns>An <see cref="ITestInspect"/> instance that will execute both functions in sequence.</returns>
    public static ITestInspect Then(this InspectAsyncFn inspectFn, InspectFn thenFn) =>
        Inspect.Start(inspectFn).Then(thenFn);
    
    /// <summary>
    /// Chains a synchronous inspect function with an asynchronous inspect function.
    /// </summary>
    /// <param name="inspectFn">The first synchronous inspect function to execute.</param>
    /// <param name="thenFn">The second asynchronous inspect function to execute after the first.</param>
    /// <returns>An <see cref="ITestInspect"/> instance that will execute both functions in sequence.</returns>
    public static ITestInspect Then(this InspectFn inspectFn, InspectAsyncFn thenFn) =>
        Inspect.Start(inspectFn).Then(thenFn);
    
    /// <summary>
    /// Chains a synchronous inspect function with another synchronous inspect function.
    /// </summary>
    /// <param name="inspectFn">The first synchronous inspect function to execute.</param>
    /// <param name="thenFn">The second synchronous inspect function to execute after the first.</param>
    /// <returns>An <see cref="ITestInspect"/> instance that will execute both functions in sequence.</returns>
    public static ITestInspect Then(this InspectFn inspectFn, InspectFn thenFn) =>
        Inspect.Start(inspectFn).Then(thenFn);
}