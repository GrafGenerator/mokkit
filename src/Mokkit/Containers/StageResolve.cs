using System;
using Mokkit.Suite;

namespace Mokkit.Containers;

/// <summary>
/// Resolves services from the test host bag. DI container adapters register this as their
/// <see cref="IStageResolve"/> implementation so stage-provided instances (e.g. mocks) can be injected into the real graph.
/// </summary>
public class StageResolve : IStageResolve
{
    private readonly ITestHostBagAccessor _testHostBagAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="StageResolve"/> class with the specified test host bag accessor.
    /// </summary>
    /// <param name="testHostBagAccessor">The test host bag accessor for accessing shared test resources.</param>
    public StageResolve(ITestHostBagAccessor testHostBagAccessor)
    {
        _testHostBagAccessor = testHostBagAccessor;
    }

    /// <summary>
    /// Resolves a service of the specified type from the test host bag.
    /// </summary>
    /// <param name="serviceType">The type of service to resolve.</param>
    /// <returns>An instance of the requested service type from the test host bag, or <c>null</c> if not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the test host bag is missing, indicating a corrupt resolver state.</exception>
    public object? Resolve(Type serviceType)
    {
        var bag = _testHostBagAccessor.Bag;

        if (bag == null)
        {
            throw new InvalidOperationException("Stage resolver is in corrupt state, test host bag is missing.");
        }

        return bag.TryGet(serviceType);
    }
}
