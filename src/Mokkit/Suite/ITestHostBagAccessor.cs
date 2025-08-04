namespace Mokkit.Suite;

/// <summary>
/// Provides access to the test host bag, which contains shared resources and state for test execution.
/// This interface allows components to access and modify the test host bag during test operations.
/// </summary>
public interface ITestHostBagAccessor
{
    /// <summary>
    /// Gets or sets the test host bag that contains shared resources and state.
    /// </summary>
    /// <value>The test host bag, or <c>null</c> if no bag is currently set.</value>
    TestHostBag? Bag { get; set; }
}