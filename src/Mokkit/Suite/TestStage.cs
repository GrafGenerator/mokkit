using System;
using System.Collections.Generic;
using Mokkit.Arrange;
using Mokkit.Containers;
using Mokkit.Inspect;

namespace Mokkit.Suite;

/// <summary>
/// Represents a test stage that provides the primary entry point for AAA (Arrange-Act-Assert) test operations.
/// This class extends <see cref="TestHost"/> to provide convenient factory methods for starting arrange and inspect chains.
/// </summary>
public class TestStage : TestHost
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestStage"/> class with the specified containers, bag accessor, and stage identifier.
    /// </summary>
    /// <param name="containers">The collection of dependency injection containers to use for service resolution.</param>
    /// <param name="bagAccessor">The test host bag accessor for accessing shared test resources.</param>
    /// <param name="stageId">The unique identifier for this test stage.</param>
    public TestStage(IEnumerable<IDependencyContainer> containers, ITestHostBagAccessor bagAccessor, Guid stageId)
        : base(containers, bagAccessor, stageId)
    {
    }

    /// <summary>
    /// Starts a new arrange operation chain for setting up test dependencies and state.
    /// This method provides the entry point for the Arrange phase of the AAA pattern.
    /// </summary>
    /// <returns>A new <see cref="ITestArrange"/> instance for chaining arrange operations.</returns>
    public ITestArrange Arrange()
    {
        return Mokkit.Arrange.Arrange.Start(this);
    }

    /// <summary>
    /// Starts a new inspect operation chain for performing assertions and verifications.
    /// This method provides the entry point for the Assert phase of the AAA pattern.
    /// </summary>
    /// <returns>A new <see cref="ITestInspect"/> instance for chaining inspect operations.</returns>
    public ITestInspect Inspect()
    {
        return Mokkit.Inspect.Inspect.Start(this);
    }
}