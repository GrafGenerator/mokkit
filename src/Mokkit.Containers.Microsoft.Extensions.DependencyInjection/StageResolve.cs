using System;
using Mokkit.Suite;

namespace Mokkit.Containers.Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides service resolution capabilities by accessing services from the test host bag.
/// This class implements <see cref="IStageResolve"/> to enable service resolution within test stage contexts.
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
    /// This method provides access to services that have been registered in the test host bag during test execution.
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