namespace Mokkit;

/// <summary>
/// Read-only contract shared by <see cref="Capture{T}"/> and <see cref="Trapture{T}"/>.
/// Exposes the captured value once it has been set by the capture system.
/// </summary>
/// <typeparam name="T">The type of the captured value.</typeparam>
public interface ICapture<out T>
{
    /// <summary>
    /// Gets the captured value, or the type default (<c>null</c> for reference types) if no value has been captured yet.
    /// </summary>
    T? Value { get; }
}
