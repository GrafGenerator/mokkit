using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mokkit.Capture.Containers;

public class MicrosoftDiContainerBuilder : IDependencyContainerBuilder
{
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();

    public Func<IServiceCollection, Task>? PreInitHook { get; set; }

    public Func<IServiceCollection, Task>? InitHook { get; set; }

    public Func<IServiceCollection, Task>? PreBuildHook { get; set; }

    public Task PreInit()
    {
        return PreInitHook?.Invoke(_serviceCollection) ?? Task.CompletedTask;
    }

    public Task Init()
    {
        return InitHook?.Invoke(_serviceCollection) ?? Task.CompletedTask;
    }

    public Task PreBuild()
    {
        return PreBuildHook?.Invoke(_serviceCollection) ?? Task.CompletedTask;
    }

    public IDependencyContainer Build()
    {
        var serviceProvider = _serviceCollection.BuildServiceProvider();

        return new MicrosoftDiContainer(serviceProvider);
    }
}