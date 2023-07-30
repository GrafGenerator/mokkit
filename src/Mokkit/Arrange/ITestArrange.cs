using Mokkit.Capture.Arrange;

namespace Mokkit.Arrange;

public interface ITestArrange
{
    ITestArrange Then(ArrangeAsyncFn arrangeFn);
    ITestArrange Then(ArrangeFn arrangeFn);
    ITestArrangeAwaiter GetAwaiter();
}