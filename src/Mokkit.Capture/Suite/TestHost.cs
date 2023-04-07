using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mokkit.Capture.Containers;

namespace Mokkit.Capture.Suite;

public class TestHost : ITestHost
{
    private readonly IEnumerable<IDependencyContainerBuilder> _builders;
    private IDependencyContainer[] _containers = Array.Empty<IDependencyContainer>();

    protected TestHost(IEnumerable<IDependencyContainerBuilder> builders)
    {
        _builders = builders;
    }

    public void Execute<TService>(Action<TService> actionFn)
    {
        using var scope = BeginScope();
        actionFn(scope.Resolve<TService>());
    }

    public void Execute<TService, TService2>(Action<TService, TService2> actionFn)
    {
        using var scope = BeginScope();
        actionFn(scope.Resolve<TService>(), scope.Resolve<TService2>());
    }

    public TOutput Execute<TService, TOutput>(Func<TService, TOutput> actionFn)
    {
        using var scope = BeginScope();
        return actionFn(scope.Resolve<TService>());
    }

    public async Task ExecuteAsync<TService>(Func<TService, Task> actionFn)
    {
        using var scope = BeginScope();
        await actionFn(scope.Resolve<TService>());
    }

    public async Task<TOutput> ExecuteAsync<TService, TOutput>(Func<TService, Task<TOutput>> actionFn)
    {
        using var scope = BeginScope();
        return await actionFn(scope.Resolve<TService>());
    }

    protected ScopeAggregator BeginScope()
    {
        return new ScopeAggregator(_containers);
    }

    protected async Task BuildContainers()
    {
        foreach (var container in _builders)
        {
            await container.PreInit();
        }

        foreach (var container in _builders)
        {
            await container.Init();
        }

        foreach (var container in _builders)
        {
            await container.PreBuild();
        }

        _containers = _builders.Select(x => x.Build()).ToArray();
    }

    protected class ScopeAggregator : IDisposable
    {
        private readonly Dictionary<Type, object> _resolveCache = new();
        private readonly List<IDependencyContainerScope> _scopes = new();

        public ScopeAggregator(IReadOnlyCollection<IDependencyContainer> containers)
        {
            foreach (var container in containers)
            {
                _scopes.Add(container.BeginScope());
            }
        }

        public T Resolve<T>()
        {
            var type = typeof(T);

            if (_resolveCache.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            var resolvedService = _scopes
                .Select(x => x.TryResolve<T>())
                .FirstOrDefault(x => x != null);
            
            if (resolvedService == null)
            {
                throw new InvalidOperationException($"Cannot find type {type} in registered containers");
            }

            _resolveCache[type] = resolvedService;

            return resolvedService;
        }

        public void Dispose()
        {
            foreach (var scope in _scopes)
            {
                scope.Dispose();
            }
        }
    }
}