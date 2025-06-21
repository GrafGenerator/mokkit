using System;

namespace Mokkit.Containers;

public interface IDependencyContainerScope : IDisposable
{
    void OnAsyncScopeEnter();

    T? TryResolve<T>() where T : class;
}