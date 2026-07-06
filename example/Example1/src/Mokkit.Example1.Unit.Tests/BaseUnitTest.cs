using Mokkit.Arrange;
using Mokkit.Inspect;
using Mokkit.Suite;

namespace Mokkit.Example1.Unit.Tests;

/// <summary>
/// Base class for the unit suite, parameterised by the per-SUT <see cref="BaseStageFixture"/>. The stage
/// composition is built once (shared by xUnit's <see cref="IClassFixture{TFixture}"/>); each test enters a
/// fresh stage in the constructor and disposes it afterwards. <see cref="Arrange"/>/<see cref="Inspect"/>
/// are the same framework-agnostic Mokkit API the integration suite uses.
/// </summary>
public abstract class BaseUnitTest<TFixture> : IClassFixture<TFixture>, IDisposable
    where TFixture : BaseStageFixture
{
    protected BaseUnitTest(TFixture fixture)
    {
        Stage = fixture.EnterStage();
    }

    protected TestStage Stage { get; }

    protected ITestArrange Arrange => Stage.Arrange();

    protected ITestInspect Inspect => Stage.Inspect();

    public void Dispose() => Stage.Dispose();
}
