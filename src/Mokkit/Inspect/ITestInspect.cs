using Mokkit.Inspect;

namespace Mokkit.Inspect;

public interface ITestInspect
{
    ITestInspect Then(InspectAsyncFn inspectFn);
    ITestInspect Then(InspectFn inspectFn);
    ITestInspectAwaiter GetAwaiter();
}