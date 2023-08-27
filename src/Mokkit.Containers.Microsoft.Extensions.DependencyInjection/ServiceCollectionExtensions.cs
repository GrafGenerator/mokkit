using System;
using Microsoft.Extensions.DependencyInjection;

namespace Mokkit.Containers.Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ResolveFromStage<T>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.ResolveFromStage(typeof(T), lifetime);
    }
    
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