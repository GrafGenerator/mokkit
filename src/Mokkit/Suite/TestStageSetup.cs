using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mokkit.Containers;

namespace Mokkit.Suite;

/// <summary>
/// Represents the setup and initialization phase for test stages, managing the lifecycle of dependency container builders.
/// This class orchestrates the multi-phase initialization process (PreInit → Init → PreBuild → Build) and creates test stages with configured containers.
/// </summary>
public class TestStageSetup
{
    private readonly IEnumerable<IDependencyContainerBuilder> _builders;
    private IDependencyContainer[] _containers = Array.Empty<IDependencyContainer>();
    private readonly TestHostBagAccessor _bagAccessor;
    private bool _areContainersBuilt;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestStageSetup"/> class with the specified dependency container builders.
    /// This constructor creates a new test host bag accessor for managing shared test resources.
    /// </summary>
    /// <param name="builders">The collection of dependency container builders to manage.</param>
    protected TestStageSetup(IEnumerable<IDependencyContainerBuilder> builders)
    {
        _builders = builders;
        _bagAccessor = new TestHostBagAccessor();
    }

    /// <summary>
    /// Creates and enters a new test stage with the configured dependency containers.
    /// This method can only be called after the containers have been successfully built through the initialization process.
    /// </summary>
    /// <returns>A new <see cref="TestStage"/> instance with the configured containers and a unique stage identifier.</returns>
    /// <exception cref="InvalidOperationException">Thrown when containers have not been built yet.</exception>
    public TestStage EnterStage()
    {
        if (!_areContainersBuilt)
        {
            throw new InvalidOperationException("Cannot enter new test stage because containers are not initialized.");
        }

        var stageId = Guid.NewGuid();

        return new TestStage(_containers, _bagAccessor, stageId);
    }

    /// <summary>
    /// Executes the multi-phase container building process for all registered dependency container builders.
    /// This method orchestrates the complete initialization lifecycle: PreInit → Init → PreBuild → Build.
    /// Each phase is executed for all builders before proceeding to the next phase to ensure proper initialization order.
    /// </summary>
    /// <returns>A task that represents the asynchronous container building operation.</returns>
    protected async Task BuildContainers()
    {
        foreach (var builder in _builders)
        {
            await builder.PreInit();
        }

        foreach (var builder in _builders)
        {
            await builder.Init();
        }

        foreach (var builder in _builders)
        {
            await builder.PreBuild(_builders.ToArray());
        }

        _containers = _builders.Select(x => x.Build(_bagAccessor)).ToArray();
        _areContainersBuilt = true;
    }

    /// <summary>
    /// Creates and initializes a new <see cref="TestStageSetup"/> instance with the specified dependency container builders.
    /// This factory method executes the complete container building process before returning the setup instance.
    /// </summary>
    /// <param name="builders">The dependency container builders to configure for the test stage setup.</param>
    /// <returns>A fully initialized <see cref="TestStageSetup"/> instance ready to create test stages.</returns>
    public static async Task<TestStageSetup> Create(params IDependencyContainerBuilder[] builders)
    {
        var setup = new TestStageSetup(builders);
        await setup.BuildContainers();

        return setup;
    }
}