using System;
using System.Threading;
using System.Threading.Tasks;
using Mokkit.Containers.Bag;
using Mokkit.Suite;

namespace Mokkit.Tests;

/// <summary>Shared test doubles used across the Mokkit self-tests.</summary>
internal interface ICounter
{
    int Next();
}

internal sealed class Counter : ICounter
{
    private int _value;

    public int Value => _value;

    public int Next() => Interlocked.Increment(ref _value);
}

internal sealed class Tracked : IDisposable
{
    public bool Disposed { get; private set; }

    public void Dispose() => Disposed = true;
}

/// <summary>Helpers to spin up a real <see cref="TestStage"/> backed by the lightweight bag container.</summary>
internal static class StageHelper
{
    public static async Task<TestStage> EmptyStage()
    {
        var setup = await TestStageSetup.Create(new BagContainerBuilder());
        return setup.EnterStage();
    }

    public static async Task<TestStage> StageWith(Action<BagContainerBuilder> configure)
    {
        var builder = new BagContainerBuilder();
        configure(builder);

        var setup = await TestStageSetup.Create(builder);
        return setup.EnterStage();
    }
}
