using System;
using System.Collections.Concurrent;
using Mokkit.Suite;
using Moq;

namespace Mokkit.Containers.Moq;

public class MoqContainer : BaseDependencyContainer, IDependencyContainer
{
    private readonly MockCollection<Mock> _mockCollection;

    internal MoqContainer(MockCollection<Mock> mockCollection)
    {
        _mockCollection = mockCollection;
    }

    public IDependencyContainerScope BeginScope(TestHostContext context)
    {
        return new MockScope(_mockCollection, context);
    }

    private class MockScope : IDependencyContainerScope
    {
        private readonly ConcurrentDictionary<Type, Mock?> _mocks = new();

        public MockScope(MockCollection<Mock> mockCollection, TestHostContext context)
        {
            var bag = context.TestHostBagResolver.Get(context.TestHostId);

            foreach (var registration in mockCollection)
            {
                var mock = registration.Factory();
                _mocks.TryAdd(mock.GetType(), mock);
                
                bag.TryAdd(registration.InnerType, mock.Object);
            }
        }
        
        public void Dispose()
        {
        }

        public T? TryResolve<T>() where T : class
        {
            
            if (_mocks.TryGetValue(typeof(T), out var mock))
            {
                return mock as T;
            }
            
            return null;
        }
    }
}