using System;
using System.Threading.Tasks;

namespace Mokkit.Containers.MockContainer;

public class MockContainerBuilder<TMock> : IDependencyContainerBuilder
{
    private readonly IMockCollection<TMock> _mockCollection = new MockCollection<TMock>();

    private Func<IMockCollection<TMock>, Task>? PreInitFn { get; set; }

    private Func<IMockCollection<TMock>, Task>? InitFn { get; set; }

    private Func<IMockCollection<TMock>, Task>? PreBuildFn { get; set; }

    Task IDependencyContainerBuilder.PreInit()
    {
        return PreInitFn != null ? PreInitFn(_mockCollection) : Task.CompletedTask;
    }

    Task IDependencyContainerBuilder.Init()
    {
        return InitFn != null ? InitFn(_mockCollection) : Task.CompletedTask;
    }

    Task IDependencyContainerBuilder.PreBuild()
    {
        return PreBuildFn != null ? PreBuildFn(_mockCollection) : Task.CompletedTask;
    }

    public MockContainerBuilder<TMock> UsePreInit(Func<IMockCollection<TMock>, Task> fn)
    {
        PreInitFn = fn;
        return this;
    }
    
    public MockContainerBuilder<TMock> UseInit(Func<IMockCollection<TMock>, Task> fn)
    {
        InitFn = fn;
        return this;
    }
    
    public MockContainerBuilder<TMock> UsePreBuild(Func<IMockCollection<TMock>, Task> fn)
    {
        PreBuildFn = fn;
        return this;
    }

    IDependencyContainer IDependencyContainerBuilder.Build()
    {
        var serviceProvider = _mockCollection.BuildServiceProvider();

        return new MicrosoftDiContainer(serviceProvider);
    }
}