using System;
using System.Collections.Generic;
using System.Linq;
using Mokkit.Containers;

namespace Mokkit.Suite;

/// <summary>
/// Internal class that aggregates multiple dependency container scopes and provides unified service resolution.
/// This class manages the lifecycle of multiple container scopes and implements caching for resolved services.
/// </summary>
internal class ScopeAggregator : IDisposable
{
    private readonly TestHostContext _context;
    private readonly Dictionary<Type, object> _resolveCache = new();
    private readonly List<IDependencyContainerScope> _scopes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScopeAggregator"/> class with the specified containers and context.
    /// Creates scopes for all provided containers within the given test host context.
    /// </summary>
    /// <param name="containers">The collection of dependency containers to aggregate.</param>
    /// <param name="context">The test host context for scope creation.</param>
    public ScopeAggregator(IReadOnlyCollection<IDependencyContainer> containers, TestHostContext context)
    {
        _context = context;

        foreach (var container in containers)
        {
            _scopes.Add(container.BeginScope(context));
        }
    }

    /// <summary>
    /// Notifies all aggregated scopes that an async scope is being entered.
    /// This method propagates the async scope entry notification to all managed container scopes.
    /// </summary>
    public void OnAsyncScopeEnter()
    {
        foreach (var scope in _scopes)
        {
            scope.OnAsyncScopeEnter();
        }
    }
    
    /// <summary>
    /// Resolves a service of type <typeparamref name="T"/> from the aggregated container scopes.
    /// This method first checks the resolve cache, then attempts resolution from each scope in order until a service is found.
    /// Successfully resolved services are cached for subsequent requests.
    /// </summary>
    /// <typeparam name="T">The type of service to resolve.</typeparam>
    /// <returns>An instance of the requested service type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the requested type cannot be resolved from any of the aggregated container scopes.</exception>
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

    /// <summary>
    /// Disposes all aggregated container scopes and releases associated resources.
    /// This method ensures proper cleanup of all managed dependency container scopes.
    /// </summary>
    public void Dispose()
    {
        foreach (var scope in _scopes)
        {
            scope.Dispose();
        }
    }
}