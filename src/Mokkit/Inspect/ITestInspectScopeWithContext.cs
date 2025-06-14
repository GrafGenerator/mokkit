namespace Mokkit.Inspect;

public interface ITestInspectScopeWithContext<out T, out TContext>: ITestInspect
{
    ITestInspectScopeWithContext<T, TContext> Then(InspectValueWithContextAsyncFn<T, TContext> inspectFn);

    ITestInspectScopeWithContext<T, TContext> Then(InspectValueWithContextFn<T, TContext> inspectFn);
}