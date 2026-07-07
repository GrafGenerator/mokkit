using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mokkit.Containers;
using Mokkit.Suite;

namespace Mokkit.Containers.Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents a builder for creating Microsoft.Extensions.DependencyInjection-based dependency injection containers.
/// This builder implements the multiphase initialization lifecycle and provides fluent configuration methods for service registration.
/// </summary>
public class ServiceProviderContainerBuilder : IDependencyContainerBuilder
{
    /// <summary>
    /// The service collection that contains all registered services.
    /// This collection is used to store and manage service registrations throughout the builder lifecycle.
    /// </summary>
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();

    /// <summary>
    /// Gets or sets the pre-initialization function that is executed during the pre-initialization phase.
    /// This function is used to perform any necessary setup or configuration before the initialization phase.
    /// </summary>
    private Func<IServiceCollection, Task>? PreInitFn { get; set; }

    /// <summary>
    /// Gets or sets the initialization function that is executed during the initialization phase.
    /// This function is used to perform service registration and configuration after the pre-initialization phase.
    /// </summary>
    private Func<IServiceCollection, Task>? InitFn { get; set; }

    /// <summary>
    /// Gets the list of pre-build functions that are executed during the pre-build phase.
    /// These functions are used to perform any necessary setup or configuration before the build phase.
    /// </summary>
    private List<Func<IServiceCollection, IDependencyContainerBuilder[], Task>> PreBuildFns { get; } = new();

    /// <summary>
    /// Executes the pre-initialization phase of the builder lifecycle.
    /// This method calls the pre-initialization function if it is set.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task IDependencyContainerBuilder.PreInit()
    {
        return PreInitFn != null ? PreInitFn(_serviceCollection) : Task.CompletedTask;
    }

    /// <summary>
    /// Executes the initialization phase of the builder lifecycle.
    /// This method calls the initialization function if it is set to configure services and their registrations.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task IDependencyContainerBuilder.Init()
    {
        return InitFn != null ? InitFn(_serviceCollection) : Task.CompletedTask;
    }

    /// <summary>
    /// Executes the pre-build phase of the builder lifecycle.
    /// This method calls all registered pre-build functions to perform cross-builder coordination.
    /// </summary>
    /// <param name="builders">An array of all container builders participating in the build process.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task IDependencyContainerBuilder.PreBuild(IDependencyContainerBuilder[] builders)
    {
        foreach (var preBuildFn in PreBuildFns)
        {
            preBuildFn(_serviceCollection, builders);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Attempts to retrieve a collection of the specified type from the builder.
    /// This method is used for accessing the service collection during the build process.
    /// </summary>
    /// <typeparam name="TCollection">The type of collection to retrieve. Must be a reference type.</typeparam>
    /// <returns>The service collection cast to the requested type, or <c>null</c> if the cast is not possible.</returns>
    public TCollection? TryGetCollection<TCollection>() where TCollection : class
    {
        return _serviceCollection as TCollection;
    }

    /// <summary>
    /// Configures a pre-initialization function to be executed during the pre-initialization phase.
    /// This method provides a fluent interface for setting up early service configuration.
    /// </summary>
    /// <param name="fn">The function to execute during pre-initialization.</param>
    /// <returns>The current <see cref="ServiceProviderContainerBuilder"/> instance for method chaining.</returns>
    public ServiceProviderContainerBuilder UsePreInit(Func<IServiceCollection, Task> fn)
    {
        PreInitFn = fn;
        return this;
    }
    
    /// <summary>
    /// Configures an initialization function to be executed during the initialization phase.
    /// This method provides a fluent interface for setting up service registration and configuration.
    /// </summary>
    /// <param name="fn">The function to execute during initialization.</param>
    /// <returns>The current <see cref="ServiceProviderContainerBuilder"/> instance for method chaining.</returns>
    public ServiceProviderContainerBuilder UseInit(Func<IServiceCollection, Task> fn)
    {
        InitFn = fn;
        return this;
    }
    
    /// <summary>
    /// Configures a pre-build function that coordinates with other container builders during the pre-build phase.
    /// This method allows for cross-builder interaction and shared configuration.
    /// </summary>
    /// <typeparam name="TCollection">The type of collection to retrieve from other builders.</typeparam>
    /// <param name="fn">The function to execute during pre-build, receiving both the service collection and another builder's collection.</param>
    /// <returns>The current <see cref="ServiceProviderContainerBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when no collection of the specified type is found among the builders.</exception>
    public ServiceProviderContainerBuilder UsePreBuild<TCollection>(Func<IServiceCollection, TCollection, Task> fn) where TCollection : class
    {
        PreBuildFns.Add((collection, builders) =>
        {
            var otherCollection = builders
                .Select(x => x.TryGetCollection<TCollection>())
                .FirstOrDefault(x => x != null) ?? throw new ArgumentException($"Cannot find collection of type {typeof(TCollection)}");

            return fn(collection, otherCollection);
            
        });
        return this;
    }

    /// <summary>
    /// Builds the final Microsoft.Extensions.DependencyInjection container using the specified bag accessor.
    /// This method registers essential services, builds the service provider, and creates the configured container instance.
    /// </summary>
    /// <param name="bagAccessor">The test host bag accessor for accessing shared test resources.</param>
    /// <returns>A configured <see cref="IDependencyContainer"/> instance containing all registered services.</returns>
    IDependencyContainer IDependencyContainerBuilder.Build(ITestHostBagAccessor bagAccessor)
    {
        _serviceCollection.AddSingleton(bagAccessor);
        _serviceCollection.AddScoped<IStageResolve, StageResolve>();
        
        var serviceProvider = _serviceCollection.BuildServiceProvider();

        return new ServiceProviderContainer(serviceProvider);
    }
}