using System;

namespace Mokkit.Capture.Containers;

public interface IDependencyContainerScope: IDisposable
{
    T? TryResolve<T>();
}