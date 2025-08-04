using System;

namespace Mokkit;

/// <summary>
/// Represents a type-safe value capture mechanism that allows capturing and retrieving values during test execution.
/// This class provides implicit conversion to the captured type and ensures type safety throughout the test lifecycle.
/// </summary>
/// <typeparam name="T">The type of value to capture.</typeparam>
public class Capture<T>: ICaptureInitializer<T>
{
    /// <summary>
    /// Gets the captured value.
    /// </summary>
    /// <value>The captured value, or <c>null</c> if no value has been captured.</value>
    public T? Value { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Capture{T}"/> class.
    /// </summary>
    internal Capture()
    {
    }

    /// <summary>
    /// Provides implicit conversion from <see cref="Capture{T}"/> to the captured type.
    /// </summary>
    /// <param name="capture">The capture instance to convert.</param>
    /// <returns>The captured value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the capture has not been initialized with a value.</exception>
    public static implicit operator T(Capture<T> capture)
    {
        return capture.Value ?? throw new InvalidOperationException("Capture is not initialized");
    }

    /// <summary>
    /// Sets the captured value. This method is called internally by the capture system.
    /// </summary>
    /// <param name="value">The value to capture.</param>
    void ICaptureInitializer<T>.Set(T value)
    {
        Value = value;
    }
}

/// <summary>
/// Provides static factory methods for creating capture instances.
/// </summary>
public static class Capture
{
    /// <summary>
    /// Starts a new capture operation and returns an initializer for setting the captured value.
    /// </summary>
    /// <typeparam name="T">The type of value to capture.</typeparam>
    /// <param name="capture">When this method returns, contains the capture instance that will hold the value.</param>
    /// <returns>An initializer that can be used to set the captured value.</returns>
    public static ICaptureInitializer<T> Start<T>(out Capture<T> capture)
    {
        capture = new Capture<T>();

        return capture;
    }
}