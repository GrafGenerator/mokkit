using Mokkit.Playground.SampleScenery;
using Moq;

namespace Mokkit.Playground.Setups
{
    public class CoreSetup: IStageSetup<string>
    {
        public void SetupMocks(IMokkit<string> mokkit)
        {
            mokkit.Customize<Mock<IService1>>(mock => mock.Setup(service1 => service1.Call1()));
            mokkit.Customize<Mock<IService2>>(mock => mock.Setup(service2 => service2.Call2()));

            var svc1 = mokkit.Resolve();
        }
    }
}