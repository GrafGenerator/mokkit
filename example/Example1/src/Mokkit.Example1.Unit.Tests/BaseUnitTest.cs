using Mokkit.Arrange;
using Mokkit.Inspect;
using Mokkit.Suite;

namespace Mokkit.Example1.Unit.Tests;

/// <summary>
/// Base class for the unit suite. The Mokkit stage is shared (built once via <see cref="StageFixture"/>);
/// each test gets a fresh stage in the constructor and disposes it afterwards. Note that
/// <see cref="Arrange"/>/<see cref="Inspect"/> are framework-agnostic — exactly the same Mokkit API the
/// NUnit integration suite uses.
/// </summary>
public abstract class BaseUnitTest : IClassFixture<StageFixture>, IDisposable
{
    protected BaseUnitTest(StageFixture fixture)
    {
        Stage = fixture.EnterStage();
    }

    protected TestStage Stage { get; }

    protected ITestArrange Arrange => Stage.Arrange();

    protected ITestInspect Inspect => Stage.Inspect();

    public void Dispose() => Stage.Dispose();
}
