using System;
using global::Autofac;

namespace Mokkit.Containers.Autofac;

/// <summary>
/// Extensions that register services to be resolved from the test stage (the shared bag) rather than from Autofac,
/// mirroring the Microsoft DI <c>ResolveFromStage</c>. Used to bridge mocks from a sibling mock container into the real graph.
/// </summary>
public static class ContainerBuilderExtensions
{
    /// <summary>Registers <typeparamref name="T"/> to be resolved from the stage.</summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <param name="builder">The Autofac container builder.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public static ContainerBuilder ResolveFromStage<T>(this ContainerBuilder builder) where T : class
        => builder.ResolveFromStage(typeof(T));

    /// <summary>Registers <paramref name="serviceType"/> to be resolved from the stage.</summary>
    /// <param name="builder">The Autofac container builder.</param>
    /// <param name="serviceType">The service type.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public static ContainerBuilder ResolveFromStage(this ContainerBuilder builder, Type serviceType)
    {
        builder.Register(ctx =>
        {
            var stageResolve = ctx.Resolve<IStageResolve>();

            return stageResolve.Resolve(serviceType)
                ?? throw new InvalidOperationException($"Cannot resolve service of type {serviceType} from stage");
        }).As(serviceType);

        return builder;
    }
}
