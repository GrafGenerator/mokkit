using System;
using System.Threading.Tasks;

namespace Mokkit.Capture.Containers;

public interface IDependencyContainerBuilder
{
    Task PreInit();
    
    Task Init();
    
    Task PreBuild();
    
    IDependencyContainer Build();
}

public interface IDependencyContainer
{
    IDependencyContainerScope BeginScope();
}

public interface IDependencyContainerScope: IDisposable
{
    T? TryResolve<T>();
}