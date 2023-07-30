using System.Collections.Generic;
using System.Threading.Tasks;
using Mokkit.Arrange;
using Mokkit.Containers;
using Mokkit.Inspect;

namespace Mokkit.Suite;

public class TestStage : TestHost
{
    private TestStage(IEnumerable<IDependencyContainerBuilder> builders) : base(builders)
    {
    }

    public ITestArrange Arrange()
    {
        return Mokkit.Arrange.Arrange.Start(this);
    }

    public ITestInspect Inspect()
    {
        return Mokkit.Inspect.Inspect.Start(this);
    }

    public static async Task<TestStage> Create(params IDependencyContainerBuilder[] builders)
    {
        var stage = new TestStage(builders);
        await stage.BuildContainers();

        return stage;
    }
}