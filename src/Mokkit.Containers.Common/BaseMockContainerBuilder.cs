using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mokkit.Containers;
using Mokkit.Suite;

namespace Mokkit.Containers.Common;

/// <summary>
/// Base builder for mock containers, implementing the multi-phase lifecycle (PreInit → Init → PreBuild → Build) and the
/// shared <see cref="MockCollection{TMock}"/>. Concrete adapters expose framework-named fluent configuration methods
/// (via <see cref="SetPreInit"/>/<see cref="SetInit"/>/<see cref="AddPreBuild{TCollection}"/>) and implement
/// <see cref="CreateContainer"/>.
/// </summary>
/// <typeparam name="TMock">The mock representation.</typeparam>
public abstract class BaseMockContainerBuilder<TMock> : IDependencyContainerBuilder
    where TMock : class
{
    /// <summary>Gets the collection of registered mocks.</summary>
    public MockCollection<TMock> MockCollection { get; } = new();

    private Func<IMockCollection<TMock>, Task>? _preInitFn;
    private Func<IMockCollection<TMock>, Task>? _initFn;
    private readonly List<Func<IMockCollection<TMock>, IDependencyContainerBuilder[], Task>> _preBuildFns = new();

    /// <summary>Sets the callback run during the pre-initialization phase.</summary>
    protected void SetPreInit(Func<IMockCollection<TMock>, Task> fn) => _preInitFn = fn;

    /// <summary>Sets the callback run during the initialization phase.</summary>
    protected void SetInit(Func<IMockCollection<TMock>, Task> fn) => _initFn = fn;

    /// <summary>
    /// Adds a pre-build callback that coordinates with a sibling builder's collection of type <typeparamref name="TCollection"/>.
    /// </summary>
    protected void AddPreBuild<TCollection>(Func<IMockCollection<TMock>, TCollection, Task> fn)
        where TCollection : class
    {
        _preBuildFns.Add((mocks, builders) =>
        {
            var other = builders
                .Select(b => b.TryGetCollection<TCollection>())
                .FirstOrDefault(x => x != null)
                ?? throw new ArgumentException($"Cannot find collection of type {typeof(TCollection)}");

            return fn(mocks, other);
        });
    }

    Task IDependencyContainerBuilder.PreInit() => _preInitFn?.Invoke(MockCollection) ?? Task.CompletedTask;

    Task IDependencyContainerBuilder.Init() => _initFn?.Invoke(MockCollection) ?? Task.CompletedTask;

    async Task IDependencyContainerBuilder.PreBuild(IDependencyContainerBuilder[] builders)
    {
        foreach (var preBuildFn in _preBuildFns)
        {
            await preBuildFn(MockCollection, builders);
        }
    }

    /// <summary>Returns the mock collection when the requested collection type matches; otherwise <c>null</c>.</summary>
    public TCollection? TryGetCollection<TCollection>() where TCollection : class => MockCollection as TCollection;

    IDependencyContainer IDependencyContainerBuilder.Build(ITestHostBagAccessor bagAccessor)
    {
        MockCollection.MakeReadOnly();

        return CreateContainer(MockCollection, bagAccessor);
    }

    /// <summary>Creates the framework-specific container from the finalized mock collection.</summary>
    protected abstract IDependencyContainer CreateContainer(IMockCollection<TMock> mocks, ITestHostBagAccessor bagAccessor);
}
