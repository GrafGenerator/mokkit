using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Mokkit.Capture;

namespace Mokkit.Playground.CaptureTests;

[TestFixture]
public class CapturePlaygroundTests
{
    [Test]
    public void TestCapture()
    {
        
    }

    private void PrepareFoo(int innerValue, out Capture<Foo> fooCapture)
    {
        var capture = Capture.Capture.Start(out fooCapture);

        capture.SetValue(new Foo(innerValue));
    }

    private async Task PrepareBar(Capture<Foo> foo, out Capture<Bar> barCapture)
    {
        var capture = Capture.Capture.Start(out barCapture);

        await ExecuteAsync(() =>
        {
            capture.SetValue(new Bar(foo));
        });
    }


    private async Task ExecuteAsync(Func<Task> action)
    {
        // do some async stuff

        await action();
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