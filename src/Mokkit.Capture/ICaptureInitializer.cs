namespace Mokkit.Capture;

public interface ICaptureInitializer<T>
{
    void SetValue(T value);
}