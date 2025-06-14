using System.Threading.Tasks;
using Mokkit.Playground.SampleScenery;
using Moq;
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
            .ArrangeFoo(out var foo, testInnerValue)
            .ArrangeBar(out var bar, foo);

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
            .ArrangeFoo(out var foo, testInnerValue)
            .ArrangeBar(out var bar, foo);

        // Act
        await Act(foo);
            
        // Assert
        await Inspect
            .Service1FooValue(Is.EqualTo(testInnerValue))
            .Service2IsCalled(Is.True);
    }

    [Test]
    public async Task TestExecuteOtherScope()
    {
        // Arrange
        var testInnerValue = 255;

        await Arrange
            .ArrangeFoo(out var foo, testInnerValue, testInnerValue.ToString());

        // Act
        await ActScoped(foo);
            
        // Assert
        await Inspect
            .Service3Invocation($"scope: {testInnerValue}", Times.Once());
    }

    [Test]
    public async Task TestMockInvocation()
    {
        // Arrange
        const int testInnerValue = 255;
        const string mockCapturedInput = "captured_mock_input";

        await Arrange
            .ArrangeFoo(out var foo, testInnerValue, mockCapturedInput)
            .ArrangeBar(out var bar, foo)
            .ArrangeMock3(mockCapturedInput, "123")
            .ArrangeMock4(mockCapturedInput, "123");

        // Act
        await Act(foo);
            
        // Assert
        await Inspect
            .Service3Invocation(mockCapturedInput, Times.Once())
            .Service4Invocation(mockCapturedInput, Times.Once());
    }
    
    [Test]
    public async Task TestScopeInspect()
    {
        const int code = 255;
        const string testValue = "test";
        const string mockCapturedInput = "captured_mock_input";
        
        // Arrange
        await Arrange
            .ArrangeSampleCommand(out var command, code: code, value: testValue)
            .ArrangeMock3(testValue, "123")
            .ArrangeMock4(testValue, "123");

        // Act
        var result = await Act(command);
            
        // Assert
        await Inspect
            .SampleResult(result)
                .IsSuccessful(code)
                .Value(testValue)
            .Service3Invocation(testValue, Times.Once())
            .Service4Invocation(testValue, Times.Once());
    }
    
    [Test]
    public async Task TestScopeInspectWithContext()
    {
        const int code = 255;
        const string testValue = "test";
        
        // Arrange
        await Arrange
            .ArrangeSampleCommand(out var command, code: code, value: testValue);

        // Act
        var result = await Act(command);
            
        // Assert
        await Inspect
            .SampleResultWithContext(result, testValue)
                .ValueFromContext();
    }
    
    private async Task<int> Act(Foo foo)
    {
        return await Stage.ExecuteAsync<SampleActor, int>(async actor => await actor.Act(foo));
    }

    private async Task<string> ActScoped(Foo foo)
    {
        return await Stage.ExecuteAsync<SampleScopedActor, string>(async actor => await actor.Act(foo));
    }
    
    private async Task<SampleResult> Act(SampleCommand command)
    {
        return await Stage.ExecuteAsync<SampleActor, SampleResult>(
            async actor => await actor.ActWithResult(command));
    }
}