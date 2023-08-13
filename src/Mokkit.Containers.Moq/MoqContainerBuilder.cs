using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;

namespace Mokkit.Containers.Moq;

public class MoqContainerBuilder : IDependencyContainerBuilder
{
    public MockCollection<Mock> MockCollection { get; } = new();

    private Func<IMockCollection<Mock>, Task>? PreInitFn { get; set; }

    private Func<IMockCollection<Mock>, Task>? InitFn { get; set; }

    private List<Func<IMockCollection<Mock>, IDependencyContainerBuilder[], Task>> PreBuildFns { get; } = new();

    Task IDependencyContainerBuilder.PreInit()
    {
        return PreInitFn != null ? PreInitFn(MockCollection) : Task.CompletedTask;
    }

    Task IDependencyContainerBuilder.Init()
    {
        return InitFn != null ? InitFn(MockCollection) : Task.CompletedTask;
    }

    Task IDependencyContainerBuilder.PreBuild(IDependencyContainerBuilder[] builders)
    {
        foreach (var preBuildFn in PreBuildFns)
        {
            preBuildFn(MockCollection, builders);
        }

        return Task.CompletedTask;
    }

    public TCollection? TryGetCollection<TCollection>() where TCollection : class
    {
        return MockCollection as TCollection;
    }

    public MoqContainerBuilder UsePreInit(Func<IMockCollection<Mock>, Task> fn)
    {
        PreInitFn = fn;
        return this;
    }
    
    public MoqContainerBuilder UseInit(Func<IMockCollection<Mock>, Task> fn)
    {
        InitFn = fn;
        return this;
    }
    
    public MoqContainerBuilder UsePreBuild<TCollection>(Func<IMockCollection<Mock>, TCollection, Task> fn) where TCollection : class
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

    IDependencyContainer IDependencyContainerBuilder.Build()
    {
        MockCollection.MakeReadOnly();
        var mockProvider = new MockProvider<Mock>(MockCollection);

        return new MoqContainer(mockProvider);
    }
}