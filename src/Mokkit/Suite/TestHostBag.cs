using System;
using System.Collections.Concurrent;

namespace Mokkit.Suite;

public class TestHostBag
{
    private readonly ConcurrentDictionary<Type, object> _bag = new();
        
    public void TryAdd(Type serviceType, object implementation)
    {
        _bag.TryAdd(serviceType, implementation);
    }
    
    public object? TryGet(Type serviceType)
    {
        return _bag.TryGetValue(serviceType, out var service) ? service : null;
    }
}