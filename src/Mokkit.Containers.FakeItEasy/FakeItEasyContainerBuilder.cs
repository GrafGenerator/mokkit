using System;
using System.Threading.Tasks;
using Mokkit.Containers;
using Mokkit.Containers.Common;
using Mokkit.Suite;

namespace Mokkit.Containers.FakeItEasy;

/// <summary>
/// Builds a <see cref="FakeItEasyContainer"/>. Register fakes on the
/// <see cref="BaseMockContainerBuilder{TMock}.MockCollection"/> — most conveniently with <c>AddFake&lt;T&gt;()</c> —
/// from the configuration callbacks.
/// </summary>
public class FakeItEasyContainerBuilder : BaseMockContainerBuilder<object>
{
    /// <summary>Configures the pre-initialization callback.</summary>
    public FakeItEasyContainerBuilder UsePreInit(Func<IMockCollection<object>, Task> fn)
    {
        SetPreInit(fn);
        return this;
    }

    /// <summary>Configures the initialization callback (typical place to register fakes).</summary>
    public FakeItEasyContainerBuilder UseInit(Func<IMockCollection<object>, Task> fn)
    {
        SetInit(fn);
        return this;
    }

    /// <summary>Configures a pre-build callback that coordinates with a sibling builder's collection.</summary>
    public FakeItEasyContainerBuilder UsePreBuild<TCollection>(Func<IMockCollection<object>, TCollection, Task> fn)
        where TCollection : class
    {
        AddPreBuild(fn);
        return this;
    }

    /// <inheritdoc />
    protected override IDependencyContainer CreateContainer(IMockCollection<object> mocks, ITestHostBagAccessor bagAccessor)
        => new FakeItEasyContainer(mocks, bagAccessor);
}
