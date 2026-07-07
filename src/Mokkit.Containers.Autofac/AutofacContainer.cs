using global::Autofac;
using Mokkit.Suite;

namespace Mokkit.Containers.Autofac;

/// <summary>
/// Mokkit container backed by Autofac. Each Mokkit scope maps to a nested Autofac lifetime scope, and resolution
/// delegates to Autofac (returning <c>null</c> for unregistered services so the aggregator can fall through).
/// </summary>
public class AutofacContainer : BaseDependencyContainer, IDependencyContainer
{
    private readonly ILifetimeScope _rootScope;

    internal AutofacContainer(ILifetimeScope rootScope)
    {
        _rootScope = rootScope;
    }

    /// <summary>Begins a nested Autofac lifetime scope.</summary>
    /// <param name="context">The test host context (unused).</param>
    /// <returns>A new scope wrapping the nested Autofac lifetime scope.</returns>
    public IDependencyContainerScope BeginScope(TestHostContext context)
        => new AutofacScope(_rootScope.BeginLifetimeScope());

    private sealed class AutofacScope : IDependencyContainerScope
    {
        private readonly ILifetimeScope _scope;

        public AutofacScope(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public void OnAsyncScopeEnter()
        {
        }

        public T? TryResolve<T>() where T : class => _scope.ResolveOptional<T>();

        public void Dispose() => _scope.Dispose();
    }
}
