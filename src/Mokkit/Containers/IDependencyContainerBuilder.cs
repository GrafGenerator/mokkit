using System.Threading.Tasks;
using Mokkit.Suite;

namespace Mokkit.Containers;

/// <summary>
/// Defines the contract for building dependency injection containers with a multiphase initialization lifecycle.
/// This interface supports a structured build process: PreInit → Init → PreBuild → Build.
/// </summary>
public interface IDependencyContainerBuilder
{
    /// <summary>
    /// Performs the pre-initialization phase of the container builder.
    /// This phase typically handles early setup tasks before the main initialization.
    /// </summary>
    /// <returns>A task representing the asynchronous pre-initialization operation.</returns>
    Task PreInit();
    
    /// <summary>
    /// Performs the main initialization phase of the container builder.
    /// This phase typically configures services and dependencies.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    Task Init();
    
    /// <summary>
    /// Performs the pre-build phase of the container builder.
    /// This phase allows for cross-builder coordination and final configuration before building.
    /// </summary>
    /// <param name="builders">An array of all container builders participating in the build process.</param>
    /// <returns>A task representing the asynchronous pre-build operation.</returns>
    Task PreBuild(IDependencyContainerBuilder[] builders);

    /// <summary>
    /// Attempts to retrieve a collection of the specified type from the builder.
    /// This method is used for accessing builder-specific collections during the build process.
    /// </summary>
    /// <typeparam name="TCollection">The type of collection to retrieve. Must be a reference type.</typeparam>
    /// <returns>The requested collection, or <c>null</c> if not available.</returns>
    TCollection? TryGetCollection<TCollection>() where TCollection : class;
    
    /// <summary>
    /// Builds the final dependency injection container using the specified bag accessor.
    /// This is the final phase that produces the configured container instance.
    /// </summary>
    /// <param name="bagAccessor">The test host bag accessor for accessing shared test resources.</param>
    /// <returns>A configured <see cref="IDependencyContainer"/> instance.</returns>
    IDependencyContainer Build(ITestHostBagAccessor bagAccessor);
}