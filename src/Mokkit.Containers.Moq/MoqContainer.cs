using System;
using Mokkit.Containers.Common;
using Mokkit.Suite;
using Moq;

namespace Mokkit.Containers.Moq;

/// <summary>
/// Mokkit container backed by Moq. Mocks are resolved by their <c>Mock&lt;T&gt;</c> wrapper type (so tests can
/// <c>Setup</c>/<c>Verify</c> them) while the underlying <c>mock.Object</c> is injected into the real graph.
/// </summary>
public class MoqContainer : BaseMockContainer<Mock>
{
    internal MoqContainer(IMockCollection<Mock> mocks, ITestHostBagAccessor bagAccessor)
        : base(mocks, bagAccessor)
    {
    }

    /// <inheritdoc />
    protected override Type GetResolveKey(Mock mock, Type innerType) => mock.GetType();

    /// <inheritdoc />
    protected override object? GetInjectable(Mock mock) => mock.Object;
}
