namespace Mokkit.Inspect;

public interface ITestInspect
{
    ITestInspect Then(InspectAsyncFn inspectFn);

    ITestInspect Then(InspectFn inspectFn);

    ITestInspectScope<T> ThenValueScope<T>(T value, InspectScopeAsyncFn? inspectScopeFn = null);

    ITestInspectScopeWithContext<T, TContext> ThenValueScope<T, TContext>(T value, TContext context,
        InspectScopeAsyncFn? inspectScopeFn = null);

    ITestInspectAwaiter GetAwaiter();
}