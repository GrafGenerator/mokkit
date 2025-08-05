using System;

namespace Mokkit.Suite;

/// <summary>
/// Represents the execution context for a test host, containing the unique identifier and shared resource bag.
/// This class provides the contextual information needed for dependency container scopes and test execution phases.
/// </summary>
public class TestHostContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestHostContext"/> class with the specified test host identifier and bag.
    /// </summary>
    /// <param name="testHostId">The unique identifier for the test host instance.</param>
    /// <param name="bag">The test host bag for storing and retrieving shared test resources.</param>
    public TestHostContext(Guid testHostId, TestHostBag bag)
    {
        TestHostId = testHostId;
        Bag = bag;
    }

    /// <summary>
    /// Gets the unique identifier for the test host instance.
    /// This identifier is used to distinguish between different test host contexts.
    /// </summary>
    public Guid TestHostId { get; }

    /// <summary>
    /// Gets the test host bag that contains shared resources and services for the test execution.
    /// This bag allows different components of the test framework to share data and services.
    /// </summary>
    public TestHostBag Bag { get; }
}