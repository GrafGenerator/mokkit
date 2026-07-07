using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace Mokkit.Containers.CastleWindsor;

/// <summary>
/// Extensions that register services to be resolved from the test stage (the shared bag) rather than from Windsor,
/// mirroring the Microsoft DI <c>ResolveFromStage</c>. Used to bridge mocks from a sibling mock container into the real graph.
/// </summary>
public static class WindsorContainerExtensions
{
    /// <summary>Registers <typeparamref name="T"/> to be resolved from the stage.</summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <param name="container">The Windsor container.</param>
    /// <returns>The container for fluent chaining.</returns>
    public static IWindsorContainer ResolveFromStage<T>(this IWindsorContainer container) where T : class
        => container.ResolveFromStage(typeof(T));

    /// <summary>Registers <paramref name="serviceType"/> to be resolved from the stage.</summary>
    /// <param name="container">The Windsor container.</param>
    /// <param name="serviceType">The service type.</param>
    /// <returns>The container for fluent chaining.</returns>
    public static IWindsorContainer ResolveFromStage(this IWindsorContainer container, Type serviceType)
    {
        container.Register(Component.For(serviceType)
            .UsingFactoryMethod(kernel =>
            {
                var stageResolve = kernel.Resolve<IStageResolve>();

                return stageResolve.Resolve(serviceType)
                    ?? throw new InvalidOperationException($"Cannot resolve service of type {serviceType} from stage");
            })
            .LifestyleTransient());

        return container;
    }
}
