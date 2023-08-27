using System;
using System.Collections.Concurrent;

namespace Mokkit.Suite;

public class TestHostBagResolver : ITestHostBagResolver
{
    private readonly ConcurrentDictionary<Guid, TestHostBag> _bags = new();
    
    public TestHostBag Get(Guid testHostId)
    {
        return _bags.TryGetValue(testHostId, out var bag)
            ? bag
            : throw new ArgumentException($"Bag for test host id '{testHostId}' is missing", nameof(testHostId));
    }

    public void Create(Guid testHostId)
    {
        _bags.TryAdd(testHostId, new TestHostBag());
    }

    public void Remove(Guid testHostId)
    {
        _bags.TryRemove(testHostId, out _);
    }
}