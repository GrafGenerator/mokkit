using System;

namespace Mokkit.Example1.Common;

/// <summary>
/// Default <see cref="IIdGenerator"/> backed by <see cref="Guid.NewGuid"/>.
/// </summary>
public sealed class GuidIdGenerator : IIdGenerator
{
    /// <inheritdoc />
    public Guid NewId() => Guid.NewGuid();
}
