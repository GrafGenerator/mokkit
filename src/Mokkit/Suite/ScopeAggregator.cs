using System;
using System.Collections.Generic;
using System.Linq;
using Mokkit.Containers;

namespace Mokkit.Suite;

internal class ScopeAggregator : IDisposable
{
    private readonly Dictionary<Type, object> _resolveCache = new();
    private readonly List<IDependencyContainerScope> _scopes = new();

    public ScopeAggregator(IReadOnlyCollection<IDependencyContainer> containers, TestHostContext context)
    {
        foreach (var container in containers)
        {
            _scopes.Add(container.BeginScope(context));
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