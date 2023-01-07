using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Mokkit.Capture;
using Mokkit.Capture.Suite;

namespace Mokkit.Playground.CaptureTests;

[TestFixture]
public class BasicPlaygroundTests: BasePlayground
{
    
    
    [SetUp]
    public void SetUp()
    {
        
    }
    
    [Test]
    public async Task TestCustomizedArrange()
    {
        // Arrange
        var testInnerValue = 1;

        await Stage.Arrange()
            .ArrangeFoo(testInnerValue, out var foo)
            .ArrangeBar(foo, out var bar);

        // Act
        
        // Assert
        Assert.That(bar.Value, Is.Not.Null);
        Assert.That(bar.Value.GetValue(), Is.EqualTo(testInnerValue));
    }
}