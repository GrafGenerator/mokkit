using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mokkit.Suite;

namespace Mokkit.Containers.Microsoft.Extensions.DependencyInjection;

public class ServiceProviderContainerBuilder : IDependencyContainerBuilder
{
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();

    private Func<IServiceCollection, Task>? PreInitFn { get; set; }

    private Func<IServiceCollection, Task>? InitFn { get; set; }

    private List<Func<IServiceCollection, IDependencyContainerBuilder[], Task>> PreBuildFns { get; } = new();

    Task IDependencyContainerBuilder.PreInit()
    {
        return PreInitFn != null ? PreInitFn(_serviceCollection) : Task.CompletedTask;
    }

    Task IDependencyContainerBuilder.Init()
    {
        return InitFn != null ? InitFn(_serviceCollection) : Task.CompletedTask;
    }

    Task IDependencyContainerBuilder.PreBuild(IDependencyContainerBuilder[] builders)
    {
        foreach (var preBuildFn in PreBuildFns)
        {
            preBuildFn(_serviceCollection, builders);
        }

        return Task.CompletedTask;
    }

    public TCollection? TryGetCollection<TCollection>() where TCollection : class
    {
        return _serviceCollection as TCollection;
    }

    public ServiceProviderContainerBuilder UsePreInit(Func<IServiceCollection, Task> fn)
    {
        PreInitFn = fn;
        return this;
    }
    
    public ServiceProviderContainerBuilder UseInit(Func<IServiceCollection, Task> fn)
    {
        InitFn = fn;
        return this;
    }
    
    public ServiceProviderContainerBuilder UsePreBuild<TCollection>(Func<IServiceCollection, TCollection, Task> fn) where TCollection : class
    {
        PreBuildFns.Add((collection, builders) =>
        {
            var otherCollection = builders
                .Select(x => x.TryGetCollection<TCollection>())
                .FirstOrDefault(x => x != null) ?? throw new ArgumentException($"Cannot find collection of type {typeof(TCollection)}");

            return fn(collection, otherCollection);
            
        });
        return this;
    }

    IDependencyContainer IDependencyContainerBuilder.Build(ITestHostBagAccessor bagAccessor)
    {
        _serviceCollection.AddSingleton(bagAccessor);
        _serviceCollection.AddScoped<IStageResolve, StageResolve>();
        
        var serviceProvider = _serviceCollection.BuildServiceProvider();

        return new ServiceProviderContainer(serviceProvider);
    }
}