using Mokkit.Containers;
using Mokkit.Suite;

namespace Mokkit.Example1.Unit.Tests.Containers;

/// <summary>
/// The built NSubstitute container. Each test scope gets a fresh set of substitutes, which it
/// publishes into the test-host bag (keyed by service interface) so real services resolved from the
/// Microsoft DI container receive them, and exposes them for direct configuration/verification.
/// </summary>
public sealed class SubstituteContainer : BaseDependencyContainer, IDependencyContainer
{
    private readonly SubstituteCollection _collection;
    private readonly ITestHostBagAccessor _bagAccessor;

    internal SubstituteContainer(SubstituteCollection collection, ITestHostBagAccessor bagAccessor)
    {
        _collection = collection;
        _bagAccessor = bagAccessor;
    }

    public IDependencyContainerScope BeginScope(TestHostContext context) =>
        new SubstituteScope(_collection, _bagAccessor);

    private sealed class SubstituteScope : IDependencyContainerScope
    {
        private readonly ITestHostBagAccessor _bagAccessor;
        private readonly Dictionary<Type, object> _substitutes = new();

        public SubstituteScope(SubstituteCollection collection, ITestHostBagAccessor bagAccessor)
        {
            _bagAccessor = bagAccessor;

            foreach (var registration in collection.Registrations)
            {
                // An NSubstitute fake IS the interface — no .Object wrapper, unlike Moq.
                _substitutes[registration.InnerType] = registration.Factory();
            }
        }

        public void OnAsyncScopeEnter()
        {
            var bag = _bagAccessor.Bag
                      ?? throw new InvalidOperationException("Scope is in corrupt state, bag is missing.");

            foreach (var (innerType, substitute) in _substitutes)
            {
                bag.TryAdd(innerType, substitute);
            }
        }

        public T? TryResolve<T>() where T : class =>
            _substitutes.TryGetValue(typeof(T), out var substitute) ? (T)substitute : null;

        public void Dispose()
        {
        }
    }
}
