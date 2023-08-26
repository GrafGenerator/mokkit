using System.Threading.Tasks;
using Mokkit.Arrange;
using Mokkit.Playground.SampleScenery;
using Moq;

namespace Mokkit.Playground.CaptureTests;

public static class SampleArrange
{
    public static ITestArrange ArrangeFoo(this ITestArrange arrange, out Capture<Foo> fooCapture, int innerValue,
        string? innerStringValue = null)
    {
        var capture = Capture.Start(out fooCapture);

        return arrange.Then(_ => { capture.Set(new Foo(innerValue, innerStringValue)); });
    }

    public static ITestArrange ArrangeBar(this ITestArrange arrange, out Capture<Bar> barCapture, Capture<Foo> foo)
    {
        var capture = Capture.Start(out barCapture);

        return arrange.Then(async _ =>
        {
            await Task.Delay(1); // async arrange
            capture.Set(new Bar(foo));
        });
    }
    
    public static ITestArrange ArrangeMock3(this ITestArrange arrange, string input, string resultOutput)
    {
        return arrange.Then(host =>
        {
            host.Execute<Mock<IService3>>(service3Mock =>
            {
                service3Mock
                    .Setup(x => x.Mocked3(It.Is<string>(s => s.Equals(input))))
                    .Returns(resultOutput);
            });
        });
    }
    
    public static ITestArrange ArrangeMock4(this ITestArrange arrange, string input, string resultOutput)
    {
        return arrange.Then(host =>
        {
            host.Execute<Mock<IService4>>(service4Mock =>
            {
                service4Mock
                    .Setup(x => x.Mocked4(It.Is<string>(s => s.Equals(input))))
                    .Returns(resultOutput);
            });
        });
    }
    
    public static ITestArrange ArrangeWithService(this ITestArrange arrange, Capture<Foo> foo, out Capture<Bar> barCapture)
    {
        var capture = Capture.Start(out barCapture);

        return arrange.Then(async host =>
        {
            await host.ExecuteAsync<Foo>(async foo1 =>
            {
                
            });
            
            await Task.Delay(1); // async arrange
            capture.Set(new Bar(foo));
        });
    }

    public static ITestArrange ArrangeSampleCommand(
        this ITestArrange arrange,
        out Capture<SampleCommand> commandCapture, bool success = true, int code = 123, string value = "123")
    {
        var capture = Capture.Start(out commandCapture);

        return arrange.Then(_ =>
        {
            capture.Set(new SampleCommand
            {
                Success = success,
                Code = code,
                Value = value
            });
        });
    }
}