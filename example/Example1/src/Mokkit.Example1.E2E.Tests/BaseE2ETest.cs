using Mokkit.Arrange;
using Mokkit.Inspect;
using Mokkit.Suite;

namespace Mokkit.Example1.E2E.Tests;

/// <summary>
/// Base for black-box E2E tests. Enters a fresh Mokkit stage per test (over the shared running stack) and
/// Respawn-resets the database afterwards. The body of every test is the same framework-agnostic Mokkit
/// Arrange/Act/Inspect — only the arranges/inspects differ: here they drive real HTTP/Kafka/DB clients.
/// </summary>
public abstract class BaseE2ETest : IAsyncLifetime
{
    private readonly E2EStack _stack;

    protected BaseE2ETest(E2EStack stack) => _stack = stack;

    protected TestStage Stage { get; private set; } = null!;

    protected ITestArrange Arrange => Stage.Arrange();

    protected ITestInspect Inspect => Stage.Inspect();

    public Task InitializeAsync()
    {
        Stage = _stack.EnterStage();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _stack.ResetDatabaseAsync();
        Stage.Dispose();
    }
}
