using System.Threading.Tasks;
using Mokkit.DiStage;
using NUnit.Framework;

namespace Mokkit.Playground.CaptureTests;

[TestFixture]
public class DiTestStagePlaygroundTests
{
    private DiTestStage _stage;
    
    [SetUp]
    public void SetUp()
    {
        _stage = new DiTestStage();
    }
    
    [Test]
    public async Task TestCustomizedArrange()
    {
        // Arrange
        var testInnerValue = 1;

        await _stage.Arrange()
            .ArrangeFoo(testInnerValue, out var foo)
            .ArrangeBar(foo, out var bar);

        // Act
        
        // Assert
        Assert.That(bar.Value, Is.Not.Null);
        Assert.That(bar.Value.GetValue(), Is.EqualTo(testInnerValue));
    }
}