using Mokkit.Containers.Common;
using global::FakeItEasy;

namespace Mokkit.Containers.FakeItEasy;

/// <summary>
/// Convenience registration methods that create FakeItEasy fakes for the mock collection.
/// </summary>
public static class FakeItEasyMockCollectionExtensions
{
    /// <summary>Registers a fake for <typeparamref name="T"/>, throwing if one is already registered.</summary>
    /// <typeparam name="T">The type to fake.</typeparam>
    /// <param name="mocks">The mock collection.</param>
    /// <returns>The collection for fluent chaining.</returns>
    public static IMockCollection<object> AddFake<T>(this IMockCollection<object> mocks) where T : class
        => mocks.AddMock<T>(() => A.Fake<T>());

    /// <summary>Registers a fake for <typeparamref name="T"/> only if one is not already registered.</summary>
    /// <typeparam name="T">The type to fake.</typeparam>
    /// <param name="mocks">The mock collection.</param>
    /// <returns>The collection for fluent chaining.</returns>
    public static IMockCollection<object> TryAddFake<T>(this IMockCollection<object> mocks) where T : class
        => mocks.TryAddMock<T>(() => A.Fake<T>());
}
