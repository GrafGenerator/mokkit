using System.Runtime.CompilerServices;

namespace Mokkit.Arrange;

public interface ITestArrangeAwaiter : INotifyCompletion
{
    bool IsCompleted { get; }
    void GetResult();
}