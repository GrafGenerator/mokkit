using System.Threading.Tasks;
using Mokkit.Arrange;
using Mokkit.Playground.SampleScenery;
using Moq;

namespace Mokkit.Playground.CaptureTests;

public static class SampleArrange
{
    public static ITestArrange ArrangeFoo(this ITestArrange arrange, int innerValue, out Capture<Foo> fooCapture)
    {
        var capture = Capture.Start(out fooCapture);
        
        return arrange.Then(_ =>
        {
            capture.Set(new Foo(innerValue));
        });
    }
    
    public static ITestArrange ArrangeBar(this ITestArrange arrange, Capture<Foo> foo, out Capture<Bar> barCapture)
    {
        var capture = Capture.Start(out barCapture);

        return arrange.Then(async _ =>
        {
            await Task.Delay(1); // async arrange
            capture.Set(new Bar(foo));
        });
    }
    
    public static ITestArrange ArrangeMock(this ITestArrange arrange, string captureInput, string resultOutput)
    {
        return arrange.Then(host =>
        {
            host.Execute<Mock<IService3>>(service3Mock =>
            {
                service3Mock
                    .Setup(x => x.Mocked(It.Is<string>(s => s.Equals(captureInput))))
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
}