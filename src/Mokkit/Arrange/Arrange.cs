using Mokkit.Suite;

namespace Mokkit.Arrange;

/// <summary>
/// Provides static factory methods for starting arrange operations in the AAA (Arrange-Act-Assert) pattern.
/// This class serves as the entry point for creating arrange chains that set up test dependencies and state.
/// </summary>
public static class Arrange
{
    /// <summary>
    /// Starts a new arrange operation from a test stage.
    /// </summary>
    /// <param name="stage">The test stage to start the arrange operation from.</param>
    /// <returns>A new <see cref="TestArrange"/> instance for chaining arrange operations.</returns>
    public static TestArrange Start(TestStage stage) => new(stage);

    /// <summary>
    /// Starts a new arrange operation with an asynchronous arrange function.
    /// </summary>
    /// <param name="arrangeFn">The asynchronous arrange function to start with.</param>
    /// <returns>A new <see cref="TestArrange"/> instance for chaining arrange operations.</returns>
    public static TestArrange Start(ArrangeAsyncFn arrangeFn) => new(arrangeFn);

    /// <summary>
    /// Starts a new arrange operation with a synchronous arrange function.
    /// </summary>
    /// <param name="arrangeFn">The synchronous arrange function to start with.</param>
    /// <returns>A new <see cref="TestArrange"/> instance for chaining arrange operations.</returns>
    public static TestArrange Start(ArrangeFn arrangeFn) => new(arrangeFn);
}