namespace Mokkit;

/// <summary>
/// Represents a type-safe, <b>explicit</b> value capture used to carry a value produced during one test phase
/// into a later one. Unlike <see cref="Trapture{T}"/>, this type does <b>not</b> convert implicitly to the
/// captured type — consumers must read <see cref="Value"/> explicitly, forcing the intent to be visible at the
/// use site.
/// </summary>
/// <typeparam name="T">The type of value to capture.</typeparam>
public class Capture<T> : ICapture<T>, ICaptureInitializer<T>
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
    /// Sets the captured value. This method is called internally by the capture system.
    /// </summary>
    /// <param name="value">The value to capture.</param>
    void ICaptureInitializer<T>.Set(T value)
    {
        Value = value;
    }
}

/// <summary>
/// Provides static factory methods for creating <see cref="Capture{T}"/> instances.
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
