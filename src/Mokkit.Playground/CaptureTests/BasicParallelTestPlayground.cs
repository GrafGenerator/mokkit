using System.Collections.Generic;
using System.Threading.Tasks;
using Mokkit.Playground.SampleScenery;
using Moq;
using NUnit.Framework;

namespace Mokkit.Playground.CaptureTests;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class BasicParallelPlaygroundTests : BaseParallelPlayground
{
    private const int ParallelTestsCasesNumber = 20;

    [Test]
    [TestCaseSource(nameof(TestExecuteOtherScopeParallelCases))]
    [Parallelizable(ParallelScope.All)]
    public async Task TestExecuteOtherScopeParallel(int tesValue)
    {
        // Arrange
        await Arrange
            .ArrangeFoo(out var foo, tesValue, tesValue.ToString());

        // Act
        await ActScoped(foo);

        // Assert
        await Inspect
            .Service3Invocation($"scope: {tesValue}", Times.Once());
    }

    public static IEnumerable<TestCaseData> TestExecuteOtherScopeParallelCases()
    {
        for (var i = 0; i < ParallelTestsCasesNumber; i++)
        {
            yield return new TestCaseData(i);
        }
    }

    private async Task<string> ActScoped(Foo foo)
    {
        return await Stage.ExecuteAsync<SampleScopedActor, string>(async actor => await actor.Act(foo));
    }
}