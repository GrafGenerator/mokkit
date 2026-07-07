using System;
using System.Threading.Tasks;
using Mokkit.Containers;
using Mokkit.Containers.Common;
using Mokkit.Suite;

namespace Mokkit.Containers.NSubstitute;

/// <summary>
/// Builds an <see cref="NSubstituteContainer"/>. Register substitutes on the
/// <see cref="BaseMockContainerBuilder{TMock}.MockCollection"/> — most conveniently with
/// <c>AddSubstitute&lt;T&gt;()</c> — from the configuration callbacks.
/// </summary>
public class NSubstituteContainerBuilder : BaseMockContainerBuilder<object>
{
    /// <summary>Configures the pre-initialization callback.</summary>
    public NSubstituteContainerBuilder UsePreInit(Func<IMockCollection<object>, Task> fn)
    {
        SetPreInit(fn);
        return this;
    }

    /// <summary>Configures the initialization callback (typical place to register substitutes).</summary>
    public NSubstituteContainerBuilder UseInit(Func<IMockCollection<object>, Task> fn)
    {
        SetInit(fn);
        return this;
    }

    /// <summary>Configures a pre-build callback that coordinates with a sibling builder's collection.</summary>
    public NSubstituteContainerBuilder UsePreBuild<TCollection>(Func<IMockCollection<object>, TCollection, Task> fn)
        where TCollection : class
    {
        AddPreBuild(fn);
        return this;
    }

    /// <inheritdoc />
    protected override IDependencyContainer CreateContainer(IMockCollection<object> mocks, ITestHostBagAccessor bagAccessor)
        => new NSubstituteContainer(mocks, bagAccessor);
}
