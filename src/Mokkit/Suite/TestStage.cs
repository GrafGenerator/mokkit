using System.Collections.Generic;
using System.Threading.Tasks;
using Mokkit.Capture.Arrange;
using Mokkit.Capture.Containers;
using Mokkit.Capture.Inspect;

namespace Mokkit.Capture.Suite;

public class TestStage : TestHost
{
    private TestStage(IEnumerable<IDependencyContainerBuilder> builders) : base(builders)
    {
    }

    public ITestArrange Arrange()
    {
        return Mokkit.Capture.Arrange.Arrange.Start(this);
    }

    public ITestInspect Inspect()
    {
        return Mokkit.Capture.Inspect.Inspect.Start(this);
    }

    public static async Task<TestStage> Create(params IDependencyContainerBuilder[] builders)
    {
        var stage = new TestStage(builders);
        await stage.BuildContainers();

        return stage;
    }
}