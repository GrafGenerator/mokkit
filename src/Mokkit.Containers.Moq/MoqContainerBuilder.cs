using System;
using System.Threading.Tasks;
using Mokkit.Containers;
using Mokkit.Containers.Common;
using Mokkit.Suite;
using Moq;

namespace Mokkit.Containers.Moq;

/// <summary>
/// Builds a <see cref="MoqContainer"/>. Register mocks on the <see cref="BaseMockContainerBuilder{TMock}.MockCollection"/>
/// from the <see cref="UseInit"/>/<see cref="UsePreInit"/> callbacks or bridge them into a DI container via <see cref="UsePreBuild{TCollection}"/>.
/// </summary>
public class MoqContainerBuilder : BaseMockContainerBuilder<Mock>
{
    /// <summary>Configures the pre-initialization callback.</summary>
    public MoqContainerBuilder UsePreInit(Func<IMockCollection<Mock>, Task> fn)
    {
        SetPreInit(fn);
        return this;
    }

    /// <summary>Configures the initialization callback (typical place to register mocks).</summary>
    public MoqContainerBuilder UseInit(Func<IMockCollection<Mock>, Task> fn)
    {
        SetInit(fn);
        return this;
    }

    /// <summary>Configures a pre-build callback that coordinates with a sibling builder's collection.</summary>
    public MoqContainerBuilder UsePreBuild<TCollection>(Func<IMockCollection<Mock>, TCollection, Task> fn)
        where TCollection : class
    {
        AddPreBuild(fn);
        return this;
    }

    /// <inheritdoc />
    protected override IDependencyContainer CreateContainer(IMockCollection<Mock> mocks, ITestHostBagAccessor bagAccessor)
        => new MoqContainer(mocks, bagAccessor);
}
