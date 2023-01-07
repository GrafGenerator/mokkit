namespace Mokkit.Capture;

public interface ITestArrange
{
    ITestArrange Then(ArrangeAsyncFn arrangeFn);
    ITestArrange Then(ArrangeFn arrangeFn);
    ITestArrangeAwaiter GetAwaiter();
}