using System.Runtime.CompilerServices;

namespace Mokkit.Inspect;

public interface ITestInspectAwaiter: INotifyCompletion
{
    bool IsCompleted { get; }
    void GetResult();
}