using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mokkit.Containers.Microsoft.Extensions.DependencyInjection;

public class MicrosoftDiContainerBuilder : IDependencyContainerBuilder
{
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();

    private Func<IServiceCollection, Task>? PreInitFn { get; set; }

    private Func<IServiceCollection, Task>? InitFn { get; set; }

    private Func<IServiceCollection, Task>? PreBuildFn { get; set; }

    Task IDependencyContainerBuilder.PreInit()
    {
        return PreInitFn != null ? PreInitFn(_serviceCollection) : Task.CompletedTask;
    }

    Task IDependencyContainerBuilder.Init()
    {
        return InitFn != null ? InitFn(_serviceCollection) : Task.CompletedTask;
    }

    Task IDependencyContainerBuilder.PreBuild()
    {
        return PreBuildFn != null ? PreBuildFn(_serviceCollection) : Task.CompletedTask;
    }

    public MicrosoftDiContainerBuilder UsePreInit(Func<IServiceCollection, Task> fn)
    {
        PreInitFn = fn;
        return this;
    }
    
    public MicrosoftDiContainerBuilder UseInit(Func<IServiceCollection, Task> fn)
    {
        InitFn = fn;
        return this;
    }
    
    public MicrosoftDiContainerBuilder UsePreBuild(Func<IServiceCollection, Task> fn)
    {
        PreBuildFn = fn;
        return this;
    }

    IDependencyContainer IDependencyContainerBuilder.Build()
    {
        var serviceProvider = _serviceCollection.BuildServiceProvider();

        return new MicrosoftDiContainer(serviceProvider);
    }
}