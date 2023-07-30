using System;

namespace Mokkit;

public class Capture<T>: ICaptureInitializer<T>
{
    public T? Value { get; private set; }

    internal Capture()
    {
    }

    public static implicit operator T(Capture<T> capture)
    {
        return capture.Value ?? throw new InvalidOperationException("Capture is not initialized");
    }

    void ICaptureInitializer<T>.Set(T value)
    {
        Value = value;
    }
}

public static class Capture
{
    public static ICaptureInitializer<T> Start<T>(out Capture<T> capture)
    {
        capture = new Capture<T>();

        return capture;
    }
}