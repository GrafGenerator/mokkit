using Mokkit.Capture.Suite;

namespace Mokkit.Capture.Inspect;

public static class Inspect
{
    public static ITestInspect Start(TestStage stage) => new TestInspect(stage);
    
    public static ITestInspect Start(InspectAsyncFn inspectFn) => new TestInspect(inspectFn);
    
    public static ITestInspect Start(InspectFn inspectFn) => new TestInspect(inspectFn);
}