namespace Mokkit.Capture.Inspect;

public interface ITestInspect
{
    ITestInspect Then(InspectAsyncFn inspectFn);
    ITestInspect Then(InspectFn inspectFn);
    ITestInspectAwaiter GetAwaiter();
}