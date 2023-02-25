using System;
using System.Threading.Tasks;

namespace Mokkit.Capture.Suite;

public interface IDependencyContainerBuilder
{
    Task PreInit();
    
    Task Init();
    
    Task PreBuild();
    
    IDependencyContainer Build();
}

public interface IDependencyContainerChain
{
    IDependencyContainerChain Next { get; }

    void SetNext(IDependencyContainerChain nextContainer);
}

public interface IDependencyContainer: IDependencyContainerChain
{
    bool CanResolve<T>();

    bool CanResolve(Type type);

    IDisposable BeginScope();

    T Resolve<T>();

    T? TryResolve<T>();
}