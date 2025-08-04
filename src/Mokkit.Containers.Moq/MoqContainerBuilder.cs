using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mokkit.Suite;
using Moq;

namespace Mokkit.Containers.Moq;

/// <summary>
/// Represents a builder for creating Moq-based dependency injection containers.
/// This builder implements the multi-phase initialization lifecycle and provides fluent configuration methods for mock setup.
/// </summary>
public class MoqContainerBuilder : IDependencyContainerBuilder
{
    /// <summary>
    /// Gets the mock collection that contains all registered mock objects.
    /// This collection is used to store and manage mock registrations throughout the builder lifecycle.
    /// </summary>
    /// <value>The mock collection containing all registered mocks.</value>
    public MockCollection<Mock> MockCollection { get; } = new();

    /// <summary>
    /// Gets or sets the pre-initialization function that is executed during the pre-initialization phase.
    /// This function is used to perform any necessary setup or configuration before the initialization phase.
    /// </summary>
    /// <value>The pre-initialization function.</value>
    private Func<IMockCollection<Mock>, Task>? PreInitFn { get; set; }

    /// <summary>
    /// Gets or sets the initialization function that is executed during the initialization phase.
    /// This function is used to perform any necessary setup or configuration after the pre-initialization phase.
    /// </summary>
    /// <value>The initialization function.</value>
    private Func<IMockCollection<Mock>, Task>? InitFn { get; set; }

    /// <summary>
    /// Gets the list of pre-build functions that are executed during the pre-build phase.
    /// These functions are used to perform any necessary setup or configuration before the build phase.
    /// </summary>
    /// <value>The list of pre-build functions.</value>
    private List<Func<IMockCollection<Mock>, IDependencyContainerBuilder[], Task>> PreBuildFns { get; } = new();

    /// <summary>
    /// Executes the pre-initialization phase of the builder lifecycle.
    /// This method is called before the initialization phase and is used to perform any necessary setup or configuration.
    /// </summary>
    /// <returns>A task that represents the pre-initialization operation.</returns>
    Task IDependencyContainerBuilder.PreInit()
    {
        return PreInitFn != null ? PreInitFn(MockCollection) : Task.CompletedTask;
    }

    /// <summary>
    /// Executes the initialization phase of the builder lifecycle.
    /// This method is called after the pre-initialization phase and is used to configure mock objects and their behaviors.
    /// </summary>
    /// <returns>A task that represents the initialization operation.</returns>
    Task IDependencyContainerBuilder.Init()
    {
        return InitFn != null ? InitFn(MockCollection) : Task.CompletedTask;
    }

    /// <summary>
    /// Executes the pre-build phase of the builder lifecycle.
    /// This method is called before the build phase and allows for cross-builder coordination and final configuration.
    /// </summary>
    /// <param name="builders">An array of all container builders participating in the build process.</param>
    /// <returns>A task that represents the pre-build operation.</returns>
    Task IDependencyContainerBuilder.PreBuild(IDependencyContainerBuilder[] builders)
    {
        foreach (var preBuildFn in PreBuildFns)
        {
            preBuildFn(MockCollection, builders);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Attempts to retrieve a collection of the specified type from the builder.
    /// This method is used for accessing the mock collection during the build process.
    /// </summary>
    /// <typeparam name="TCollection">The type of collection to retrieve. Must be a reference type.</typeparam>
    /// <returns>The mock collection cast to the requested type, or <c>null</c> if the cast is not possible.</returns>
    public TCollection? TryGetCollection<TCollection>() where TCollection : class
    {
        return MockCollection as TCollection;
    }

    /// <summary>
    /// Configures a pre-initialization function to be executed during the pre-initialization phase.
    /// This method provides a fluent interface for setting up early mock configuration.
    /// </summary>
    /// <param name="fn">The function to execute during pre-initialization.</param>
    /// <returns>The current <see cref="MoqContainerBuilder"/> instance for method chaining.</returns>
    public MoqContainerBuilder UsePreInit(Func<IMockCollection<Mock>, Task> fn)
    {
        PreInitFn = fn;
        return this;
    }
    
    /// <summary>
    /// Configures an initialization function to be executed during the initialization phase.
    /// This method provides a fluent interface for setting up mock configuration and behaviors.
    /// </summary>
    /// <param name="fn">The function to execute during initialization.</param>
    /// <returns>The current <see cref="MoqContainerBuilder"/> instance for method chaining.</returns>
    public MoqContainerBuilder UseInit(Func<IMockCollection<Mock>, Task> fn)
    {
        InitFn = fn;
        return this;
    }
    
    /// <summary>
    /// Configures a pre-build function that coordinates with other container builders during the pre-build phase.
    /// This method allows for cross-builder interaction and shared configuration.
    /// </summary>
    /// <typeparam name="TCollection">The type of collection to retrieve from other builders.</typeparam>
    /// <param name="fn">The function to execute during pre-build, receiving both the mock collection and another builder's collection.</param>
    /// <returns>The current <see cref="MoqContainerBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when no collection of the specified type is found among the builders.</exception>
    public MoqContainerBuilder UsePreBuild<TCollection>(Func<IMockCollection<Mock>, TCollection, Task> fn) where TCollection : class
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
    /// Builds the final Moq dependency injection container using the specified bag accessor.
    /// This method finalizes the mock collection and creates the configured container instance.
    /// </summary>
    /// <param name="bagAccessor">The test host bag accessor for accessing shared test resources.</param>
    /// <returns>A configured <see cref="IDependencyContainer"/> instance containing all registered mocks.</returns>
    IDependencyContainer IDependencyContainerBuilder.Build(ITestHostBagAccessor bagAccessor)
    {
        MockCollection.MakeReadOnly();

        return new MoqContainer(MockCollection, bagAccessor);
    }
}