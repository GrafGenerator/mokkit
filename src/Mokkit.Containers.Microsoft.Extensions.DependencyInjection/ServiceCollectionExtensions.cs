using System;
using Microsoft.Extensions.DependencyInjection;

namespace Mokkit.Containers.Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to enable stage-based service resolution.
/// These extensions allow services to be resolved from the test stage context rather than the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a service of type <typeparamref name="T"/> to be resolved from the test stage context.
    /// This method allows services to be resolved from the test host bag rather than the DI container.
    /// </summary>
    /// <typeparam name="T">The type of service to register for stage resolution.</typeparam>
    /// <param name="services">The service collection to add the registration to.</param>
    /// <param name="lifetime">The service lifetime for the registration. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection ResolveFromStage<T>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.ResolveFromStage(typeof(T), lifetime);
    }
    
    /// <summary>
    /// Registers a service of the specified type to be resolved from the test stage context.
    /// This method creates a service descriptor that uses <see cref="IStageResolve"/> to resolve services from the test host bag.
    /// </summary>
    /// <param name="services">The service collection to add the registration to.</param>
    /// <param name="serviceType">The type of service to register for stage resolution.</param>
    /// <param name="lifetime">The service lifetime for the registration. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service cannot be resolved from the stage context.</exception>
    public static IServiceCollection ResolveFromStage(
        this IServiceCollection services,
        Type serviceType,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        var descriptor = new ServiceDescriptor(serviceType, sp =>
        {
            var stageResolve = sp.GetRequiredService<IStageResolve>();

            return stageResolve.Resolve(serviceType) ??
                   throw new InvalidOperationException($"Cannot resolve service of type {serviceType}");
        }, lifetime);

        services.Add(descriptor);

        return services;
    }
}