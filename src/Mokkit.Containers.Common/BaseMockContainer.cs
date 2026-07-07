using System;
using System.Collections.Concurrent;
using Mokkit.Containers;
using Mokkit.Suite;

namespace Mokkit.Containers.Common;

/// <summary>
/// Base implementation of a mock container shared by the framework-specific adapters (Moq, NSubstitute, FakeItEasy).
/// It materializes one mock per registration per scope, exposes them for direct resolution, and deposits the
/// injectable object into the shared <see cref="TestHostBag"/> so sibling DI containers can wire them into the real graph.
/// Adapters supply the two framework-specific hooks: how a mock is keyed for resolution and how its injectable object is obtained.
/// </summary>
/// <typeparam name="TMock">The mock representation (e.g. Moq's <c>Mock</c> wrapper, or <c>object</c> when the substitute is the object).</typeparam>
public abstract class BaseMockContainer<TMock> : BaseDependencyContainer, IDependencyContainer
    where TMock : class
{
    private readonly IMockCollection<TMock> _mocks;
    private readonly ITestHostBagAccessor _bagAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseMockContainer{TMock}"/> class.
    /// </summary>
    /// <param name="mocks">The registered mocks.</param>
    /// <param name="bagAccessor">The test host bag accessor for cross-container sharing.</param>
    protected BaseMockContainer(IMockCollection<TMock> mocks, ITestHostBagAccessor bagAccessor)
    {
        _mocks = mocks;
        _bagAccessor = bagAccessor;
    }

    /// <summary>
    /// Returns the type under which a materialized mock is resolved. For a wrapper-based framework this is the wrapper
    /// type (e.g. <c>Mock&lt;T&gt;</c>); for a framework whose substitute is the object itself this is the mocked type.
    /// </summary>
    /// <param name="mock">The materialized mock.</param>
    /// <param name="innerType">The mocked type recorded at registration.</param>
    /// <returns>The resolution key type.</returns>
    protected abstract Type GetResolveKey(TMock mock, Type innerType);

    /// <summary>
    /// Returns the object to inject into the real graph (deposited into the bag). For a wrapper-based framework this is
    /// the produced object (e.g. <c>mock.Object</c>); for a substitute-is-object framework this is the mock itself.
    /// </summary>
    /// <param name="mock">The materialized mock.</param>
    /// <returns>The injectable object, or <c>null</c>.</returns>
    protected abstract object? GetInjectable(TMock mock);

    /// <summary>
    /// Begins a new mock scope, materializing one instance of every registered mock.
    /// </summary>
    /// <param name="context">The test host context (unused — the container has no per-context state).</param>
    /// <returns>A new mock scope.</returns>
    public IDependencyContainerScope BeginScope(TestHostContext context)
        => new MockScope(_mocks, _bagAccessor, GetResolveKey, GetInjectable);

    private sealed class MockScope : IDependencyContainerScope
    {
        private readonly ITestHostBagAccessor _bagAccessor;
        private readonly Func<TMock, object?> _getInjectable;
        private readonly ConcurrentDictionary<Type, Entry> _instances = new();

        public MockScope(
            IMockCollection<TMock> mocks,
            ITestHostBagAccessor bagAccessor,
            Func<TMock, Type, Type> getResolveKey,
            Func<TMock, object?> getInjectable)
        {
            _bagAccessor = bagAccessor;
            _getInjectable = getInjectable;

            foreach (var registration in mocks)
            {
                var mock = registration.Factory();
                _instances.TryAdd(getResolveKey(mock, registration.InnerType), new Entry(mock, registration.InnerType));
            }
        }

        public void OnAsyncScopeEnter()
        {
            var bag = _bagAccessor.Bag
                ?? throw new InvalidOperationException("Scope is in corrupt state, bag is missing.");

            foreach (var entry in _instances.Values)
            {
                var injectable = _getInjectable(entry.Mock);

                if (injectable != null)
                {
                    bag.TryAdd(entry.InnerType, injectable);
                }
            }
        }

        public T? TryResolve<T>() where T : class
            => _instances.TryGetValue(typeof(T), out var entry) ? entry.Mock as T : null;

        public void Dispose()
        {
        }

        private readonly struct Entry
        {
            public Entry(TMock mock, Type innerType)
            {
                Mock = mock;
                InnerType = innerType;
            }

            public TMock Mock { get; }

            public Type InnerType { get; }
        }
    }
}
