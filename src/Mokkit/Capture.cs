using System;

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
    /// Gets the captured value, guarded — <c>capture.EnsureValue</c> instead of
    /// <c>capture.Value!</c>. Fails loudly when the capture is unfilled or its value is empty.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the capture is unfilled or its value is empty.</exception>
    public T EnsureValue => CaptureGuard.EnsureValue(Value, "Capture");

    /// <summary>
    /// Reads a member off the captured value — <c>client.Prop(c =&gt; c.Id)</c> instead of
    /// <c>client.Value!.Id</c>.
    /// </summary>
    /// <typeparam name="TProp">The type of the member being read.</typeparam>
    /// <param name="propFn">Projects the member from the (non-null) captured value.</param>
    /// <returns>The projected member, which may itself be <c>null</c> if the member is nullable.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propFn"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the capture has not been initialized with a value.</exception>
    public TProp? Prop<TProp>(Func<T, TProp> propFn)
    {
        return CaptureGuard.Prop(Value, propFn, "Capture");
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
