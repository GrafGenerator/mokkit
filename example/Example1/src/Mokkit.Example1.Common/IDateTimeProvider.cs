using System;

namespace Mokkit.Example1.Common;

/// <summary>
/// Abstraction over the system clock so that time-dependent logic can be made
/// deterministic in tests.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Current UTC time.
    /// </summary>
    DateTime UtcNow { get; }
}
