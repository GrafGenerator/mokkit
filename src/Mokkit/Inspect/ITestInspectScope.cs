namespace Mokkit.Inspect;

public interface ITestInspectScope<out T>: ITestInspect
{
    ITestInspectScope<T> Then(InspectValueAsyncFn<T> inspectFn);

    ITestInspectScope<T> Then(InspectValueFn<T> inspectFn);
}