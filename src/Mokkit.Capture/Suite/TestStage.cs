using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mokkit.Capture.Suite;

public class TestStage : ITestHost
{
    private readonly IEnumerable<IDependencyContainerBuilder> _builders;
    private IDependencyContainer[] _containers = Array.Empty<IDependencyContainer>();

    public TestStage(IEnumerable<IDependencyContainerBuilder> builders)
    {
        _builders = builders;
    }

    public async Task BuildContainers()
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

    public ITestArrange Arrange()
    {
        return Mokkit.Capture.Arrange.Start(this);
    }

    public void ExecuteAsync<TService>(Action<TService> actionFn)
    {
        var types = GetContainerMap(typeof(TService));
        using var scope = new ScopeAggregator(types);

        actionFn(scope.Resolve<TService>());
    }

    public void ExecuteAsync<TService, TService2>(Action<TService, TService2> actionFn)
    {
        var types = GetContainerMap(typeof(TService), typeof(TService2));
        using var scope = new ScopeAggregator(types);

        actionFn(scope.Resolve<TService>(), scope.Resolve<TService2>());
    }

    public TOutput ExecuteAsync<TService, TOutput>(Func<TService, TOutput> actionFn)
    {
        var types = GetContainerMap(typeof(TService));
        using var scope = new ScopeAggregator(types);

        return actionFn(scope.Resolve<TService>());
    }

    public async Task ExecuteAsync<TService>(Func<TService, Task> actionFn)
    {
        var container = FindContainer<TService>();
        using var scope = container.BeginScope();

        var service = container.Resolve<TService>();

        await actionFn(service);
    }

    public async Task<TOutput> ExecuteAsync<TService, TOutput>(Func<TService, Task<TOutput>> actionFn)
    {
        var container = FindContainer<TService>();
        using var scope = container.BeginScope();

        var service = container.Resolve<TService>();

        return await actionFn(service);
    }

    private IDependencyContainer FindContainer<T>()
    {
        var container = _containers
            .FirstOrDefault(x => x.CanResolve<T>());

        return container ?? throw new InvalidOperationException($"Cannot find container for type {typeof(T)}");
    }

    private IReadOnlyCollection<TypeContainerPair> GetContainerMap(params Type[] types)
    {
        var typeContainerPairs = types
            .Select(x =>
            {
                var container = _containers.FirstOrDefault(c => c.CanResolve(x)) ??
                                     throw new InvalidOperationException($"Cannot find container for type {x}");
                return new TypeContainerPair(x, container);
            })
            .ToArray();

        return typeContainerPairs;
    }

    private record TypeContainerPair(Type Type, IDependencyContainer Container)
    {
        public Type Type { get; } = Type;

        public IDependencyContainer Container { get; } = Container;
    }

    private class ScopeAggregator : IDisposable
    {
        private readonly Dictionary<Type, IDependencyContainer> _typeContainerMap;
        private readonly List<IDisposable> _scopes = new();

        public ScopeAggregator(IReadOnlyCollection<TypeContainerPair> typeMap)
        {
            var containerMap = typeMap
                .Select(x => x.Container)
                .Distinct()
                .ToDictionary(x => x.GetType());

            _typeContainerMap = typeMap.ToDictionary(
                x => x.Type,
                x => containerMap[x.Container.GetType()]);

            foreach (var container in containerMap.Values)
            {
                _scopes.Add(container.BeginScope());
            }
        }

        public T Resolve<T>()
        {
            if (!_typeContainerMap.TryGetValue(typeof(T), out var container))
            {
                throw new InvalidOperationException($"Cannot find container for type {typeof(T)}");
            }

            return container.Resolve<T>();
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