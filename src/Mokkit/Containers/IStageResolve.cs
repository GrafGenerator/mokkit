using System;

namespace Mokkit.Containers;

/// <summary>
/// Provides service resolution capabilities within a test stage context, backed by the shared test host bag.
/// DI container adapters register an implementation so services can be resolved from mocks (or other stage-provided
/// instances) deposited into the bag by sibling containers.
/// </summary>
public interface IStageResolve
{
    /// <summary>
    /// Resolves a service of the specified type from the stage context.
    /// </summary>
    /// <param name="serviceType">The type of service to resolve.</param>
    /// <returns>An instance of the requested service type, or <c>null</c> if the service is not registered.</returns>
    object? Resolve(Type serviceType);
}
