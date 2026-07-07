using System;
using System.Collections.Generic;

namespace Mokkit.Containers.Common;

/// <summary>
/// Represents a collection of mock registrations that provides fluent registration methods for mock objects.
/// This interface extends <see cref="IList{T}"/> to provide list functionality while adding mock-specific registration methods.
/// </summary>
/// <typeparam name="TMock">The type of mock objects managed by this collection.</typeparam>
public interface IMockCollection<TMock> : IList<MockRegistration<TMock>>
{
    /// <summary>
    /// Adds a mock registration for the specified type with the provided factory function.
    /// If a registration for the type already exists, this method will replace it.
    /// </summary>
    /// <typeparam name="T">The type to register the mock for.</typeparam>
    /// <param name="factory">The factory function that creates the mock instance.</param>
    /// <returns>The current <see cref="IMockCollection{TMock}"/> instance for method chaining.</returns>
    IMockCollection<TMock> AddMock<T>(Func<TMock> factory);

    /// <summary>
    /// Attempts to add a mock registration for the specified type with the provided factory function.
    /// If a registration for the type already exists, this method will not replace it.
    /// </summary>
    /// <typeparam name="T">The type to register the mock for.</typeparam>
    /// <param name="factory">The factory function that creates the mock instance.</param>
    /// <returns>The current <see cref="IMockCollection{TMock}"/> instance for method chaining.</returns>
    IMockCollection<TMock> TryAddMock<T>(Func<TMock> factory);

    /// <summary>
    /// Gets a read-only collection of all mock registrations in this collection.
    /// </summary>
    /// <value>A read-only collection containing all registered mock registrations.</value>
    IReadOnlyCollection<MockRegistration<TMock>> Registrations { get; }
}
