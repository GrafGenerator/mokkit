using System;

namespace Mokkit.Containers;

public interface IDependencyContainerScope: IDisposable
{
    T? TryResolve<T>() where T : class;
}