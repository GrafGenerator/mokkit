using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mokkit.Containers;
using Mokkit.Suite;

namespace Mokkit.Containers.Bag;

/// <summary>
/// A minimal dependency container that resolves objects by their registered type. It performs no auto-wiring and
/// has no external DI dependency: singletons are stored as-is and shared across scopes, while factory registrations
/// produce one value per scope that is disposed when the scope ends.
/// </summary>
public class BagContainer : BaseDependencyContainer, IDependencyContainer
{
    private readonly IReadOnlyDictionary<Type, BagRegistration> _registrations;

    /// <summary>
    /// Initializes a new instance of the <see cref="BagContainer"/> class with the specified registrations.
    /// </summary>
    /// <param name="registrations">The type-keyed registrations produced by the builder.</param>
    internal BagContainer(IReadOnlyDictionary<Type, BagRegistration> registrations)
    {
        _registrations = registrations;
    }

    /// <summary>
    /// Creates a new resolution scope. Singleton registrations resolve to the shared instance; factory
    /// registrations produce (and cache) one value for the lifetime of the returned scope.
    /// </summary>
    /// <param name="context">The test host context (unused — the bag has no per-context state).</param>
    /// <returns>A new <see cref="IDependencyContainerScope"/>.</returns>
    public IDependencyContainerScope BeginScope(TestHostContext context) => new BagScope(_registrations);

    /// <summary>
    /// A resolution scope over a set of bag registrations. Factory-produced values are cached per scope and
    /// disposed on <see cref="Dispose"/>; singleton instances are left untouched (owned by the caller).
    /// </summary>
    private sealed class BagScope : IDependencyContainerScope
    {
        private readonly IReadOnlyDictionary<Type, BagRegistration> _registrations;
        private readonly ConcurrentDictionary<Type, object> _scoped = new();

        public BagScope(IReadOnlyDictionary<Type, BagRegistration> registrations)
        {
            _registrations = registrations;
        }

        /// <summary>No-op — the bag has no async scope-entry behavior.</summary>
        public void OnAsyncScopeEnter()
        {
        }

        /// <summary>
        /// Resolves a value of type <typeparamref name="T"/>, or <c>null</c> if the type is not registered.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>The resolved value, or <c>null</c> when not registered.</returns>
        public T? TryResolve<T>() where T : class
        {
            if (!_registrations.TryGetValue(typeof(T), out var registration))
            {
                return null;
            }

            if (registration.IsSingleton)
            {
                return (T?)registration.Instance;
            }

            return (T)_scoped.GetOrAdd(typeof(T), _ => registration.Factory!());
        }

        /// <summary>Disposes every factory-produced value that implements <see cref="IDisposable"/>.</summary>
        public void Dispose()
        {
            foreach (var instance in _scoped.Values)
            {
                if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _scoped.Clear();
        }
    }
}
