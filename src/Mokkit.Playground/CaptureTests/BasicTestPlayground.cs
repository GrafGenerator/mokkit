using System.Threading.Tasks;
using Mokkit.Playground.SampleScenery;
using NUnit.Framework;

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
    
    [Test]
    public async Task TestExecuteInScope()
    {
        // Arrange
        var testInnerValue = 255;

        await Arrange
            .ArrangeFoo(testInnerValue, out var foo)
            .ArrangeBar(foo, out var bar);

        // Act
        await Act(foo);
            
        // Assert
        await Inspect
            .Service1FooValue(Is.EqualTo(testInnerValue))
            .Service2IsCalled(Is.True);
    }

    private async Task<int> Act(Foo foo)
    {
        return await Stage.ExecuteAsync<SampleActor, int>(async actor => await actor.Act(foo));
    }
}