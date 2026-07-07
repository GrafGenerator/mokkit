using Castle.Windsor;
using Mokkit.Suite;

namespace Mokkit.Containers.CastleWindsor;

/// <summary>
/// Mokkit container backed by Castle Windsor. Resolution delegates to the Windsor container, returning <c>null</c>
/// for unregistered services so the aggregator can fall through.
/// </summary>
public class CastleWindsorContainer : BaseDependencyContainer, IDependencyContainer
{
    private readonly IWindsorContainer _container;

    internal CastleWindsorContainer(IWindsorContainer container)
    {
        _container = container;
    }

    /// <summary>Begins a resolution scope over the Windsor container.</summary>
    /// <param name="context">The test host context (unused).</param>
    /// <returns>A new scope.</returns>
    public IDependencyContainerScope BeginScope(TestHostContext context) => new WindsorScope(_container);

    private sealed class WindsorScope : IDependencyContainerScope
    {
        private readonly IWindsorContainer _container;

        public WindsorScope(IWindsorContainer container)
        {
            _container = container;
        }

        public void OnAsyncScopeEnter()
        {
        }

        public T? TryResolve<T>() where T : class
            => _container.Kernel.HasComponent(typeof(T)) ? _container.Resolve<T>() : null;

        public void Dispose()
        {
        }
    }
}
