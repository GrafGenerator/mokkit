using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Mokkit.Suite;

namespace Mokkit.Containers.CastleWindsor;

/// <summary>
/// Builds a <see cref="CastleWindsorContainer"/>. Register components on the underlying Windsor container from the
/// <see cref="UseInit"/>/<see cref="UsePreInit"/> callbacks, or bridge mocks in via <see cref="UsePreBuild{TCollection}"/>
/// and <c>IWindsorContainer.ResolveFromStage(...)</c>.
/// </summary>
public class CastleWindsorContainerBuilder : IDependencyContainerBuilder
{
    private readonly IWindsorContainer _container = new WindsorContainer();
    private Func<IWindsorContainer, Task>? _preInitFn;
    private Func<IWindsorContainer, Task>? _initFn;
    private readonly List<Func<IWindsorContainer, IDependencyContainerBuilder[], Task>> _preBuildFns = new();

    /// <summary>Configures the pre-initialization callback.</summary>
    public CastleWindsorContainerBuilder UsePreInit(Func<IWindsorContainer, Task> fn)
    {
        _preInitFn = fn;
        return this;
    }

    /// <summary>Configures the initialization callback (typical place to register components).</summary>
    public CastleWindsorContainerBuilder UseInit(Func<IWindsorContainer, Task> fn)
    {
        _initFn = fn;
        return this;
    }

    /// <summary>Configures a pre-build callback that coordinates with a sibling builder's collection.</summary>
    public CastleWindsorContainerBuilder UsePreBuild<TCollection>(Func<IWindsorContainer, TCollection, Task> fn)
        where TCollection : class
    {
        _preBuildFns.Add((container, builders) =>
        {
            var other = builders
                .Select(b => b.TryGetCollection<TCollection>())
                .FirstOrDefault(x => x != null)
                ?? throw new ArgumentException($"Cannot find collection of type {typeof(TCollection)}");

            return fn(container, other);
        });

        return this;
    }

    Task IDependencyContainerBuilder.PreInit() => _preInitFn?.Invoke(_container) ?? Task.CompletedTask;

    Task IDependencyContainerBuilder.Init() => _initFn?.Invoke(_container) ?? Task.CompletedTask;

    async Task IDependencyContainerBuilder.PreBuild(IDependencyContainerBuilder[] builders)
    {
        foreach (var preBuildFn in _preBuildFns)
        {
            await preBuildFn(_container, builders);
        }
    }

    /// <summary>Returns the underlying Windsor container when requested; otherwise <c>null</c>.</summary>
    public TCollection? TryGetCollection<TCollection>() where TCollection : class => _container as TCollection;

    IDependencyContainer IDependencyContainerBuilder.Build(ITestHostBagAccessor bagAccessor)
    {
        _container.Register(Component.For<ITestHostBagAccessor>().Instance(bagAccessor));
        _container.Register(Component.For<IStageResolve>().ImplementedBy<StageResolve>().LifestyleTransient());

        return new CastleWindsorContainer(_container);
    }
}
