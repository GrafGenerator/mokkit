using System.Threading.Tasks;
using Mokkit.Inspect;
using Mokkit.Playground.SampleScenery;
using Moq;
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
    
    public static ITestInspect Service3Invocation(this ITestInspect inspect, string capturedInput, Times times)
    {
        return inspect.Then(host =>
        {
            host.Execute<Mock<IService3>>(service3Mock =>
            {
                service3Mock.Verify(x => x.Mocked3(It.Is<string>(s => s.Equals(capturedInput))), times);
            });
        });
    }
    
    public static ITestInspect Service4Invocation(this ITestInspect inspect, string capturedInput, Times times)
    {
        return inspect.Then(host =>
        {
            host.Execute<Mock<IService4>>(service4Mock =>
            {
                service4Mock.Verify(x => x.Mocked4(It.Is<string>(s => s.Equals(capturedInput))), times);
            });
        });
    }
    
    public static ITestInspectScope<SampleResult> SampleResult(this ITestInspect inspect, SampleResult result)
    {
        return inspect.ThenValueScope(result, async (_, execute) =>
        {
            Assert.Multiple(async () => await execute());
        });
    }
    
    public static ITestInspectScope<SampleResult> IsSuccessful(this ITestInspectScope<SampleResult> inspect, int code)
    {
        return inspect.Then((result, _) =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Code, Is.EqualTo(code));
        });
    }
    
    public static ITestInspectScope<SampleResult> Value(this ITestInspectScope<SampleResult> inspect, string value)
    {
        return inspect.Then((result, _) =>
        {
            Assert.That(result.Value, Is.EqualTo(value));
        });
    }
    
    public static ITestInspectScopeWithContext<SampleResult, string> SampleResultWithContext(this ITestInspect inspect, SampleResult result, string value)
    {
        return inspect.ThenValueScope(result, value, async (_, execute) =>
        {
            Assert.Multiple(async () => await execute());
        });
    }
    
    public static ITestInspectScopeWithContext<SampleResult, string> ValueFromContext(this ITestInspectScopeWithContext<SampleResult, string> inspect)
    {
        return inspect.Then((result, context, _) =>
        {
            Assert.That(result.Value, Is.EqualTo(context));
        });
    }
}