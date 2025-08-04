using System;

namespace Mokkit.Containers;

/// <summary>
/// Represents a scoped dependency injection container that provides service resolution within a specific scope.
/// This interface extends <see cref="IDisposable"/> to ensure proper cleanup of scoped resources.
/// </summary>
public interface IDependencyContainerScope : IDisposable
{
    /// <summary>
    /// Notifies the scope that an asynchronous operation is entering the scope.
    /// This method is called to ensure proper scope management in async scenarios.
    /// </summary>
    void OnAsyncScopeEnter();

    /// <summary>
    /// Attempts to resolve a service of the specified type from the dependency injection container.
    /// </summary>
    /// <typeparam name="T">The type of service to resolve. Must be a reference type.</typeparam>
    /// <returns>An instance of the requested service type, or <c>null</c> if the service is not registered.</returns>
    T? TryResolve<T>() where T : class;
}