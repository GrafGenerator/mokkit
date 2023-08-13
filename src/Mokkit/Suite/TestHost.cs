using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mokkit.Containers;

namespace Mokkit.Suite;

public class TestHost : ITestHost
{
    private readonly IEnumerable<IDependencyContainerBuilder> _builders;
    private IDependencyContainer[] _containers = Array.Empty<IDependencyContainer>();

    protected TestHost(IEnumerable<IDependencyContainerBuilder> builders)
    {
        _builders = builders;
    }

    public void Execute<TService>(Action<TService> actionFn)
        where TService : class
    {
        using var scope = BeginScope();
        actionFn(scope.Resolve<TService>());
    }
    
    public void Execute<TService, TService2>(Action<TService, TService2> actionFn)
        where TService : class
        where TService2 : class
    {
        using var scope = BeginScope();
        actionFn(scope.Resolve<TService>(), scope.Resolve<TService2>());
    }

    public void Execute<TService, TService2, TService3>(Action<TService, TService2, TService3> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
    {
        using var scope = BeginScope();
        actionFn(scope.Resolve<TService>(), scope.Resolve<TService2>(), scope.Resolve<TService3>());
    }
    
    public void Execute<TService, TService2, TService3, TService4>(Action<TService, TService2, TService3, TService4> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
    {
        using var scope = BeginScope();
        actionFn(scope.Resolve<TService>(), scope.Resolve<TService2>(), scope.Resolve<TService3>(), scope.Resolve<TService4>());
    }
    
    public TOutput Execute<TService, TOutput>(Func<TService, TOutput> actionFn)
        where TService : class
    {
        using var scope = BeginScope();
        return actionFn(scope.Resolve<TService>());
    }

    public TOutput Execute<TService, TService2, TOutput>(Func<TService, TService2, TOutput> actionFn)
        where TService : class
        where TService2 : class
    {
        using var scope = BeginScope();
        return actionFn(scope.Resolve<TService>(), scope.Resolve<TService2>());
    }

    public TOutput Execute<TService, TService2, TService3, TOutput>(Func<TService, TService2, TService3, TOutput> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
    {
        using var scope = BeginScope();
        return actionFn(scope.Resolve<TService>(), scope.Resolve<TService2>(), scope.Resolve<TService3>());
    }
    
    public TOutput Execute<TService, TService2, TService3, TService4, TOutput>(Func<TService, TService2, TService3, TService4, TOutput> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
    {
        using var scope = BeginScope();
        return actionFn(scope.Resolve<TService>(), scope.Resolve<TService2>(), scope.Resolve<TService3>(), scope.Resolve<TService4>());
    }
    
    public async Task ExecuteAsync<TService>(Func<TService, Task> actionFn)
        where TService : class
    {
        using var scope = BeginScope();
        await actionFn(scope.Resolve<TService>());
    }

    public async Task ExecuteAsync<TService, TService2>(Func<TService, TService2, Task> actionFn)
        where TService : class
        where TService2 : class
    {
        using var scope = BeginScope();
        await actionFn(scope.Resolve<TService>(), scope.Resolve<TService2>());
    }

    public async Task ExecuteAsync<TService, TService2, TService3>(Func<TService, TService2, TService3, Task> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
    {
        using var scope = BeginScope();
        await actionFn(scope.Resolve<TService>(), scope.Resolve<TService2>(), scope.Resolve<TService3>());
    }
    
    public async Task ExecuteAsync<TService, TService2, TService3, TService4>(Func<TService, TService2, TService3, TService4, Task> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
    {
        using var scope = BeginScope();
        await actionFn(scope.Resolve<TService>(), scope.Resolve<TService2>(), scope.Resolve<TService3>(), scope.Resolve<TService4>());
    }
    
    public async Task<TOutput> ExecuteAsync<TService, TOutput>(Func<TService, Task<TOutput>> actionFn)
        where TService : class
    {
        using var scope = BeginScope();
        return await actionFn(scope.Resolve<TService>());
    }

    public async Task<TOutput> ExecuteAsync<TService, TService2, TOutput>(Func<TService, TService2, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class
    {
        using var scope = BeginScope();
        return await actionFn(scope.Resolve<TService>(), scope.Resolve<TService2>());
    }

    public async Task<TOutput> ExecuteAsync<TService, TService2, TService3, TOutput>(Func<TService, TService2, TService3, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
    {
        using var scope = BeginScope();
        return await actionFn(scope.Resolve<TService>(), scope.Resolve<TService2>(), scope.Resolve<TService3>());
    }
    
    public async Task<TOutput> ExecuteAsync<TService, TService2, TService3, TService4, TOutput>(Func<TService, TService2, TService3, TService4, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
    {
        using var scope = BeginScope();
        return await actionFn(scope.Resolve<TService>(), scope.Resolve<TService2>(), scope.Resolve<TService3>(), scope.Resolve<TService4>());
    }
    
    protected ScopeAggregator BeginScope()
    {
        return new ScopeAggregator(_containers);
    }

    protected async Task BuildContainers()
    {
        foreach (var builder in _builders)
        {
            await builder.PreInit();
        }

        foreach (var builder in _builders)
        {
            await builder.Init();
        }

        foreach (var builder in _builders)
        {
            await builder.PreBuild(_builders.ToArray());
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

        public T Resolve<T>() where T : class
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