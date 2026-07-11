using Mokkit.Suite;

namespace Mokkit.Act;

/// <summary>
/// Provides static factory methods for starting act operations in the AAA (Arrange-Act-Assert) pattern.
/// This class serves as the entry point for creating act chains that perform the operation(s) under test.
/// </summary>
public static class Act
{
    /// <summary>
    /// Starts a new act operation from a test stage.
    /// </summary>
    /// <param name="stage">The test stage to start the act operation from.</param>
    /// <returns>A new <see cref="TestAct"/> instance for chaining act operations.</returns>
    public static TestAct Start(TestStage stage) => new(stage);
}
