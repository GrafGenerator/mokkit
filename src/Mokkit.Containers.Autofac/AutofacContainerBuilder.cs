using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using global::Autofac;
using Mokkit.Suite;

namespace Mokkit.Containers.Autofac;

/// <summary>
/// Builds an <see cref="AutofacContainer"/>. Register components on the underlying Autofac <c>ContainerBuilder</c>
/// from the <see cref="UseInit"/>/<see cref="UsePreInit"/> callbacks, or bridge mocks in via <see cref="UsePreBuild{TCollection}"/>
/// and <c>ContainerBuilder.ResolveFromStage(...)</c>.
/// </summary>
public class AutofacContainerBuilder : IDependencyContainerBuilder
{
    private readonly ContainerBuilder _builder = new();
    private Func<ContainerBuilder, Task>? _preInitFn;
    private Func<ContainerBuilder, Task>? _initFn;
    private readonly List<Func<ContainerBuilder, IDependencyContainerBuilder[], Task>> _preBuildFns = new();

    /// <summary>Configures the pre-initialization callback.</summary>
    public AutofacContainerBuilder UsePreInit(Func<ContainerBuilder, Task> fn)
    {
        _preInitFn = fn;
        return this;
    }

    /// <summary>Configures the initialization callback (typical place to register components).</summary>
    public AutofacContainerBuilder UseInit(Func<ContainerBuilder, Task> fn)
    {
        _initFn = fn;
        return this;
    }

    /// <summary>Configures a pre-build callback that coordinates with a sibling builder's collection.</summary>
    public AutofacContainerBuilder UsePreBuild<TCollection>(Func<ContainerBuilder, TCollection, Task> fn)
        where TCollection : class
    {
        _preBuildFns.Add((builder, builders) =>
        {
            var other = builders
                .Select(b => b.TryGetCollection<TCollection>())
                .FirstOrDefault(x => x != null)
                ?? throw new ArgumentException($"Cannot find collection of type {typeof(TCollection)}");

            return fn(builder, other);
        });

        return this;
    }

    Task IDependencyContainerBuilder.PreInit() => _preInitFn?.Invoke(_builder) ?? Task.CompletedTask;

    Task IDependencyContainerBuilder.Init() => _initFn?.Invoke(_builder) ?? Task.CompletedTask;

    async Task IDependencyContainerBuilder.PreBuild(IDependencyContainerBuilder[] builders)
    {
        foreach (var preBuildFn in _preBuildFns)
        {
            await preBuildFn(_builder, builders);
        }
    }

    /// <summary>Returns the underlying Autofac <c>ContainerBuilder</c> when requested; otherwise <c>null</c>.</summary>
    public TCollection? TryGetCollection<TCollection>() where TCollection : class => _builder as TCollection;

    IDependencyContainer IDependencyContainerBuilder.Build(ITestHostBagAccessor bagAccessor)
    {
        _builder.RegisterInstance(bagAccessor).As<ITestHostBagAccessor>();
        _builder.RegisterType<StageResolve>().As<IStageResolve>();

        var container = _builder.Build();

        return new AutofacContainer(container);
    }
}
