using System;

namespace Mokkit.Capture;

public class Capture<T>: ICaptureInitializer<T>
{
    private T? _value;

    internal Capture()
    {
    }

    public static implicit operator T(Capture<T> capture)
    {
        return capture._value ?? throw new InvalidOperationException("Capture is not initialized");
    }

    void ICaptureInitializer<T>.SetValue(T value)
    {
        _value = value;
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