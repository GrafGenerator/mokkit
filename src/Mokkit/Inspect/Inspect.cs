using Mokkit.Suite;

namespace Mokkit.Inspect;

/// <summary>
/// Provides static factory methods for starting inspect operations in the AAA (Arrange-Act-Assert) pattern.
/// This class serves as the entry point for creating inspect chains that perform assertions and verifications.
/// </summary>
public static class Inspect
{
    /// <summary>
    /// Starts a new inspect operation from a test stage.
    /// </summary>
    /// <param name="stage">The test stage to start the inspect operation from.</param>
    /// <returns>A new <see cref="ITestInspect"/> instance for chaining inspect operations.</returns>
    public static ITestInspect Start(TestStage stage) => new TestInspect(stage);

    /// <summary>
    /// Starts a new inspect operation with an asynchronous inspect function.
    /// </summary>
    /// <param name="inspectFn">The asynchronous inspect function to start with.</param>
    /// <returns>A new <see cref="ITestInspect"/> instance for chaining inspect operations.</returns>
    public static ITestInspect Start(InspectAsyncFn inspectFn) => new TestInspect(inspectFn);

    /// <summary>
    /// Starts a new inspect operation with a synchronous inspect function.
    /// </summary>
    /// <param name="inspectFn">The synchronous inspect function to start with.</param>
    /// <returns>A new <see cref="ITestInspect"/> instance for chaining inspect operations.</returns>
    public static ITestInspect Start(InspectFn inspectFn) => new TestInspect(inspectFn);
}