using System.Threading.Tasks;
using Mokkit.Capture.Containers;
using Mokkit.Capture.Suite;
using NUnit.Framework;

namespace Mokkit.Playground.CaptureTests;

public class BasePlayground
{
    protected TestStage Stage = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var builders = new IDependencyContainerBuilder[]{
            new MicrosoftDiContainerBuilder()
        };
        
        Stage = await TestStage.Create(builders);
    }
}