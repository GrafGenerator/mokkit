using System;
using Mokkit.Containers.Common;
using Mokkit.Suite;

namespace Mokkit.Containers.NSubstitute;

/// <summary>
/// Mokkit container backed by NSubstitute. An NSubstitute substitute <b>is</b> the object, so it is resolved by the
/// mocked type and injected into the real graph as-is.
/// </summary>
public class NSubstituteContainer : BaseMockContainer<object>
{
    internal NSubstituteContainer(IMockCollection<object> mocks, ITestHostBagAccessor bagAccessor)
        : base(mocks, bagAccessor)
    {
    }

    /// <inheritdoc />
    protected override Type GetResolveKey(object mock, Type innerType) => innerType;

    /// <inheritdoc />
    protected override object? GetInjectable(object mock) => mock;
}
