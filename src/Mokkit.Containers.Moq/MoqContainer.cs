using System;
using System.Collections.Concurrent;
using Mokkit.Suite;
using Moq;

namespace Mokkit.Containers.Moq;

/// <summary>
/// Represents a dependency injection container that provides mock object resolution using the Moq framework.
/// This container integrates with the Mokkit testing framework to provide scoped mock management and resolution.
/// </summary>
public class MoqContainer : BaseDependencyContainer, IDependencyContainer
{
    private readonly MockCollection<Mock> _mockCollection;
    private readonly ITestHostBagAccessor _bagAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoqContainer"/> class with the specified mock collection and bag accessor.
    /// </summary>
    /// <param name="mockCollection">The collection of mock registrations to use for object resolution.</param>
    /// <param name="bagAccessor">The test host bag accessor for accessing shared test resources.</param>
    internal MoqContainer(MockCollection<Mock> mockCollection, ITestHostBagAccessor bagAccessor)
    {
        _mockCollection = mockCollection;
        _bagAccessor = bagAccessor;
    }

    /// <summary>
    /// Creates a new mock scope within the specified test host context.
    /// The scope initializes all registered mocks and provides isolated mock resolution for the duration of a test operation.
    /// </summary>
    /// <param name="context">The test host context that defines the scope parameters.</param>
    /// <returns>A new <see cref="IDependencyContainerScope"/> for scoped mock resolution.</returns>
    public IDependencyContainerScope BeginScope(TestHostContext context)
    {
        return new MockScope(_mockCollection, _bagAccessor, context);
    }

    /// <summary>
    /// Represents a scoped mock container that provides isolated mock object resolution within a specific test context.
    /// This class manages the lifecycle of mock objects and integrates them with the test host bag for shared access.
    /// </summary>
    private class MockScope : IDependencyContainerScope
    {
        private readonly ITestHostBagAccessor _bagAccessor;
        private readonly TestHostContext _context;
        private readonly ConcurrentDictionary<Type, (Mock? Mock, Type InnerType)> _mocks = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MockScope"/> class with the specified mock collection, bag accessor, and context.
        /// </summary>
        /// <param name="mockCollection">The collection of mock registrations to initialize within this scope.</param>
        /// <param name="bagAccessor">The test host bag accessor for accessing shared test resources.</param>
        /// <param name="context">The test host context that defines the scope parameters.</param>
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

        /// <summary>
        /// Notifies the scope that an asynchronous operation is entering the scope.
        /// This method adds all mock objects to the test host bag for shared access across the test execution.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the test host bag is missing, indicating a corrupt scope state.</exception>
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// This implementation does not require cleanup as mock objects are managed by the Moq framework.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Attempts to resolve a mock object of the specified type from the mock container.
        /// </summary>
        /// <typeparam name="T">The type of mock object to resolve. Must be a reference type.</typeparam>
        /// <returns>A mock instance of the requested type, or <c>null</c> if no mock is registered for the type.</returns>
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