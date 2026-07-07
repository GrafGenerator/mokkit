using System;

namespace Mokkit;

/// <summary>
/// TRansparent cAPTURE — a value capture that converts <b>implicitly</b> to the captured type, so it can be
/// passed wherever a <typeparamref name="T"/> is expected without reading <see cref="Value"/> explicitly.
/// Use this when the captured value flows transparently into later Arrange/Act steps; use
/// <see cref="Capture{T}"/> instead when you want to force an explicit <c>.Value</c> read.
/// </summary>
/// <typeparam name="T">The type of value to capture.</typeparam>
public class Trapture<T> : ICapture<T>, ICaptureInitializer<T>
{
    /// <summary>
    /// Gets the captured value.
    /// </summary>
    /// <value>The captured value, or <c>null</c> if no value has been captured.</value>
    public T? Value { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Trapture{T}"/> class.
    /// </summary>
    internal Trapture()
    {
    }

    /// <summary>
    /// Provides implicit conversion from <see cref="Trapture{T}"/> to the captured type.
    /// </summary>
    /// <param name="trapture">The trapture instance to convert.</param>
    /// <returns>The captured value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the trapture has not been initialized with a value.</exception>
    public static implicit operator T(Trapture<T> trapture)
    {
        return trapture.Value ?? throw new InvalidOperationException("Trapture is not initialized");
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
/// Provides static factory methods for creating <see cref="Trapture{T}"/> instances.
/// </summary>
public static class Trapture
{
    /// <summary>
    /// Starts a new transparent capture operation and returns an initializer for setting the captured value.
    /// </summary>
    /// <typeparam name="T">The type of value to capture.</typeparam>
    /// <param name="trapture">When this method returns, contains the trapture instance that will hold the value.</param>
    /// <returns>An initializer that can be used to set the captured value.</returns>
    public static ICaptureInitializer<T> Start<T>(out Trapture<T> trapture)
    {
        trapture = new Trapture<T>();

        return trapture;
    }
}
