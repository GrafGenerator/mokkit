using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mokkit.Containers;

namespace Mokkit.Suite;

public class TestStageSetup
{
    private readonly IEnumerable<IDependencyContainerBuilder> _builders;
    private IDependencyContainer[] _containers = Array.Empty<IDependencyContainer>();
    private readonly TestHostBagAccessor _bagAccessor;
    private bool _areContainersBuilt;

    protected TestStageSetup(IEnumerable<IDependencyContainerBuilder> builders)
    {
        _builders = builders;
        _bagAccessor = new TestHostBagAccessor();
    }

    public TestStage EnterStage()
    {
        if (!_areContainersBuilt)
        {
            throw new InvalidOperationException("Cannot enter new test stage because containers are not initialized.");
        }

        var stageId = Guid.NewGuid();

        return new TestStage(_containers, _bagAccessor, stageId);
    }

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

    public static async Task<TestStageSetup> Create(params IDependencyContainerBuilder[] builders)
    {
        var setup = new TestStageSetup(builders);
        await setup.BuildContainers();

        return setup;
    }
}