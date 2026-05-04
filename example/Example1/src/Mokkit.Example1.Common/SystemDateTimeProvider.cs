using System;

namespace Mokkit.Example1.Common;

/// <summary>
/// Default <see cref="IDateTimeProvider"/> backed by the real system clock.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
