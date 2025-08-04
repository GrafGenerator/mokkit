using System;

namespace Mokkit.Containers.Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides service resolution capabilities within a test stage context.
/// This interface abstracts service resolution to allow for flexible dependency injection strategies during test execution.
/// </summary>
public interface IStageResolve
{
    /// <summary>
    /// Resolves a service of the specified type from the dependency injection container.
    /// </summary>
    /// <param name="serviceType">The type of service to resolve.</param>
    /// <returns>An instance of the requested service type, or <c>null</c> if the service is not registered.</returns>
    object? Resolve(Type serviceType);
}