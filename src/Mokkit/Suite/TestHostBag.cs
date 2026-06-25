using System;
using System.Collections.Concurrent;

namespace Mokkit.Suite;

/// <summary>
/// Represents a thread-safe collection for storing and retrieving test-related objects by their type.
/// This class provides a type-based storage mechanism that allows test components to share data and services across test execution phases.
/// </summary>
public class TestHostBag
{
    private readonly ConcurrentDictionary<Type, object> _bag = new();
        
    /// <summary>
    /// Attempts to add an object to the bag with the specified service type as the key.
    /// If an object with the same type already exists, this method will not replace it.
    /// </summary>
    /// <param name="serviceType">The type to use as the key for storing the object.</param>
    /// <param name="implementation">The object instance to store in the bag.</param>
    public void TryAdd(Type serviceType, object implementation)
    {
        _bag.TryAdd(serviceType, implementation);
    }
    
    /// <summary>
    /// Attempts to retrieve an object from the bag using the specified service type as the key.
    /// </summary>
    /// <param name="serviceType">The type to use as the key for retrieving the object.</param>
    /// <returns>The object associated with the specified type, or <c>null</c> if no object is found.</returns>
    public object? TryGet(Type serviceType)
    {
        return _bag.TryGetValue(serviceType, out var service) ? service : null;
    }
}