using System;

namespace Mokkit.Example1.Common;

/// <summary>
/// Abstraction over identifier generation so that generated ids can be made
/// deterministic in tests.
/// </summary>
public interface IIdGenerator
{
    /// <summary>
    /// Produces a new identifier.
    /// </summary>
    Guid NewId();
}
