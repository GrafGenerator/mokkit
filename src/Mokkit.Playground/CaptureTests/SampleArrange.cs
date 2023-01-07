using System.Threading.Tasks;
using Mokkit.Capture;

namespace Mokkit.Playground.CaptureTests;

public static class SampleArrange
{
    public static ITestArrange ArrangeFoo(this ITestArrange arrange, int innerValue, out Capture<Foo> fooCapture)
    {
        var capture = Capture.Capture.Start(out fooCapture);
        
        return arrange.Then(() =>
        {
            capture.Set(new Foo(innerValue));
        });
    }
    
    public static ITestArrange ArrangeBar(this ITestArrange arrange, Capture<Foo> foo, out Capture<Bar> barCapture)
    {
        var capture = Capture.Capture.Start(out barCapture);

        return arrange.Then(async () =>
        {
            await Task.Delay(1); // async arrange
            capture.Set(new Bar(foo));
        });
    }
}