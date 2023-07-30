using System.Runtime.CompilerServices;

namespace Mokkit.Capture.Inspect;

public interface ITestInspectAwaiter: INotifyCompletion
{
    bool IsCompleted { get; }
    void GetResult();
}