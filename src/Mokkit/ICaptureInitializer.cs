namespace Mokkit;

/// <summary>
/// Defines the contract for initializing capture instances with values.
/// This interface is used internally by the capture system to set captured values in a type-safe manner.
/// </summary>
/// <typeparam name="T">The type of value that can be set in the capture.</typeparam>
public interface ICaptureInitializer<T>
{
    /// <summary>
    /// Sets the value in the capture instance.
    /// </summary>
    /// <param name="value">The value to set in the capture.</param>
    void Set(T value);
}