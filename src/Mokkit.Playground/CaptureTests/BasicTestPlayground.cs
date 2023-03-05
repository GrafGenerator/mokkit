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

        await Stage.Arrange()
            .ArrangeFoo(testInnerValue, out var foo)
            .ArrangeBar(foo, out var bar);

        // Act
        await Stage.ExecuteAsync<SampleActor>(async actor =>
        {
            await actor.Act(foo);
        });
            
        // Assert
        Stage.Execute<IService2>( service =>
        {
            Assert.That(service.IsCalled(), Is.True);
        });
    }
}