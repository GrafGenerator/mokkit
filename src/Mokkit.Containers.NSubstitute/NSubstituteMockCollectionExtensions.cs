using Mokkit.Containers.Common;
using global::NSubstitute;

namespace Mokkit.Containers.NSubstitute;

/// <summary>
/// Convenience registration methods that create NSubstitute substitutes for the mock collection.
/// </summary>
public static class NSubstituteMockCollectionExtensions
{
    /// <summary>Registers a substitute for <typeparamref name="T"/>, throwing if one is already registered.</summary>
    /// <typeparam name="T">The type to substitute.</typeparam>
    /// <param name="mocks">The mock collection.</param>
    /// <returns>The collection for fluent chaining.</returns>
    public static IMockCollection<object> AddSubstitute<T>(this IMockCollection<object> mocks) where T : class
        => mocks.AddMock<T>(() => Substitute.For<T>());

    /// <summary>Registers a substitute for <typeparamref name="T"/> only if one is not already registered.</summary>
    /// <typeparam name="T">The type to substitute.</typeparam>
    /// <param name="mocks">The mock collection.</param>
    /// <returns>The collection for fluent chaining.</returns>
    public static IMockCollection<object> TryAddSubstitute<T>(this IMockCollection<object> mocks) where T : class
        => mocks.TryAddMock<T>(() => Substitute.For<T>());
}
