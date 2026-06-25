using System;

namespace Mokkit.Containers.Moq;

/// <summary>
/// Represents a registration entry for a mock object, containing the type information and factory function.
/// This class encapsulates the metadata and creation logic for a specific mock registration.
/// </summary>
/// <typeparam name="TMock">The type of mock object that this registration creates.</typeparam>
public class MockRegistration<TMock>
{
    /// <summary>
    /// Gets the inner type that this mock registration represents.
    /// This is typically the interface or class type that the mock implements.
    /// </summary>
    /// <value>The type that this mock registration is for.</value>
    public Type InnerType { get; }

    /// <summary>
    /// Gets the factory function that creates instances of the mock object.
    /// </summary>
    /// <value>A function that returns a new instance of the mock object when invoked.</value>
    public Func<TMock> Factory { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockRegistration{TMock}"/> class with the specified inner type and factory function.
    /// </summary>
    /// <param name="innerType">The type that this mock registration represents.</param>
    /// <param name="factory">The factory function that creates instances of the mock object.</param>
    public MockRegistration(Type innerType, Func<TMock> factory)
    {
        InnerType = innerType;
        Factory = factory;
    }
}