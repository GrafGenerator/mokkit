using System.Threading.Tasks;
using Mokkit.Capture.Inspect;
using Mokkit.Playground.SampleScenery;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Mokkit.Playground.CaptureTests;

public static class SampleInspect
{
    public static ITestInspect Service2IsCalled(this ITestInspect inspect, IResolveConstraint expression)
    {
        return inspect.Then(async host =>
        {
            await host.ExecuteAsync<IService2>(async service2 =>
            {
                Assert.That(service2.IsCalled, expression);
            });
        });
    }
    
    public static ITestInspect Service1FooValue(this ITestInspect inspect, IResolveConstraint expression)
    {
        return inspect.Then(async host =>
        {
            await host.ExecuteAsync<IService1>(async service1 =>
            {
                Assert.That(service1.Value, expression);
            });
        });
    }
}