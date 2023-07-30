namespace Mokkit.Capture.Inspect;

public static class InspectFnExtensions
{
    public static ITestInspect Then(this InspectAsyncFn inspectFn, InspectAsyncFn thenFn) =>
        Inspect.Start(inspectFn).Then(thenFn);
    
    public static ITestInspect Then(this InspectAsyncFn inspectFn, InspectFn thenFn) =>
        Inspect.Start(inspectFn).Then(thenFn);
    
    public static ITestInspect Then(this InspectFn inspectFn, InspectAsyncFn thenFn) =>
        Inspect.Start(inspectFn).Then(thenFn);
    
    public static ITestInspect Then(this InspectFn inspectFn, InspectFn thenFn) =>
        Inspect.Start(inspectFn).Then(thenFn);
}