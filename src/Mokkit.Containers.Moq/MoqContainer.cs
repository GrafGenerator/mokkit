using System;
using System.Collections.Concurrent;
using Mokkit.Suite;
using Moq;

namespace Mokkit.Containers.Moq;

public class MoqContainer : BaseDependencyContainer, IDependencyContainer
{
    private readonly MockCollection<Mock> _mockCollection;
    private readonly ITestHostBagAccessor _bagAccessor;

    internal MoqContainer(MockCollection<Mock> mockCollection, ITestHostBagAccessor bagAccessor)
    {
        _mockCollection = mockCollection;
        _bagAccessor = bagAccessor;
    }

    public IDependencyContainerScope BeginScope(TestHostContext context)
    {
        return new MockScope(_mockCollection, _bagAccessor, context);
    }

    private class MockScope : IDependencyContainerScope
    {
        private readonly ITestHostBagAccessor _bagAccessor;
        private readonly TestHostContext _context;
        private readonly ConcurrentDictionary<Type, (Mock? Mock, Type InnerType)> _mocks = new();

        public MockScope(MockCollection<Mock> mockCollection, ITestHostBagAccessor bagAccessor, TestHostContext context)
        {
            _bagAccessor = bagAccessor;
            _context = context;

            foreach (var registration in mockCollection)
            {
                var mock = registration.Factory();
                _mocks.TryAdd(mock.GetType(), (mock, registration.InnerType));
            }
        }

        public void OnAsyncScopeEnter()
        {
            var bag = _bagAccessor.Bag;

            if (bag == null)
            {
                throw new InvalidOperationException("Scope is in corrupt state, bag is missing.");
            }
            
            foreach (var mock in _mocks.Values)
            {
                bag.TryAdd(mock.InnerType, mock.Mock?.Object);
            }
        }

        public void Dispose()
        {
        }

        public T? TryResolve<T>() where T : class
        {
            if (_mocks.TryGetValue(typeof(T), out var mock))
            {
                return mock.Mock as T;
            }

            return null;
        }
    }
}