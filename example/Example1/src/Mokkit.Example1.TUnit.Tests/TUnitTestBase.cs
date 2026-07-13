using Mokkit.Arrange;
using Mokkit.Inspect;
using Mokkit.Suite;

namespace Mokkit.Example1.TUnit.Tests;

/// <summary>
/// Base for the TUnit suite. TUnit has no <c>IClassFixture</c>/<c>IAsyncLifetime</c>: a per-class fixture is
/// injected via <c>[ClassDataSource]</c> on the concrete class, and per-test setup/teardown use
/// <c>[Before(Test)]</c> / <c>[After(Test)]</c> hooks (inherited from this base). The Mokkit primitives are
/// identical to the xUnit/NUnit suites — only these framework hooks differ.
/// </summary>
public abstract class TUnitTestBase
{
    protected TestStage Stage { get; private set; } = null!;

    protected ITestArrange Arrange => Stage.Arrange();
    protected ITestInspect Inspect => Stage.Inspect();

    /// <summary>Concrete classes return a fresh stage from their injected fixture.</summary>
    protected abstract TestStage EnterStage();

    [Before(Test)]
    public void CreateStage() => Stage = EnterStage();

    [After(Test)]
    public void DisposeStage() => Stage.Dispose();
}
