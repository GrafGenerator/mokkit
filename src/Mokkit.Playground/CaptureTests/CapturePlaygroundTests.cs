using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Mokkit.Capture;

namespace Mokkit.Playground.CaptureTests;

[TestFixture]
public class CapturePlaygroundTests
{
    [Test]
    public async Task TestCapture()
    {
        // Arrange
        var testInnerValue = 1;
        
        var arrange = ArrangeFoo(testInnerValue, out var foo)
            .Then(ArrangeBar(foo, out var bar));

        await ArrangeAsync(arrange);
        
        // Act
        
        // Assert
        Assert.That(bar.Value, Is.Not.Null);
        Assert.That(bar.Value.GetValue(), Is.EqualTo(testInnerValue));
    }

    private ArrangeFn ArrangeFoo(int innerValue, out Capture<Foo> fooCapture)
    {
        var capture = Capture.Capture.Start(out fooCapture);
        
        return () =>
        {
            capture.Set(new Foo(innerValue));
        };
    }

    private ArrangeAsyncFn ArrangeBar(Capture<Foo> foo, out Capture<Bar> barCapture)
    {
        var capture = Capture.Capture.Start(out barCapture);

        return async () =>
        {
            await Task.Delay(1); // async arrange
            capture.Set(new Bar(foo));
        };
    }


    private async Task ArrangeAsync(ITestArrange arrange)
    {
        if (arrange is not ITestArrangeProvider provider)
        {
            throw new InvalidOperationException("Specified test arrange has no provider implementation");
        }

        var arrangeFns = provider.GetArrangeFunctions();

        foreach (var arrangeFn in arrangeFns)
        {
            await arrangeFn();
        }
    }
    
    private class Foo
    {
        public int Value { get; }

        public Foo(int value)
        {
            Value = value;
        }
    }

    private class Bar
    {
        public Foo Foo { get; }

        public Bar(Foo foo)
        {
            Foo = foo;
        }

        public int GetValue() => Foo.Value;
    }
}