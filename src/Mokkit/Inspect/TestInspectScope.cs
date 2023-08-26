using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mokkit.Inspect;

internal class TestInspectScope<T> : ITestInspectScope<T>
{
    private readonly List<InspectValueAsyncFn<T>> _innerFns;
    private readonly TestInspect _parent;

    public TestInspectScope(List<InspectValueAsyncFn<T>> innerFns, TestInspect parent)
    {
        _innerFns = innerFns;
        _parent = parent;
    }

    public ITestInspectScope<T> Then(InspectValueAsyncFn<T> inspectFn)
    {
        _innerFns.Add(inspectFn);
        return this;
    }

    public ITestInspectScope<T> Then(InspectValueFn<T> inspectFn)
    {
        _innerFns.Add((value, host) =>
        {
            inspectFn(value, host);
            return Task.CompletedTask;
        });

        return this;
    }

    public ITestInspect Then(InspectAsyncFn inspectFn)
    {
        return _parent.Then(inspectFn);
    }

    public ITestInspect Then(InspectFn inspectFn)
    {
        return _parent.Then(inspectFn);
    }

    public ITestInspectScope<T1> ThenValueScope<T1>(T1 value, InspectScopeAsyncFn? inspectScopeFn = null)
    {
        throw new InvalidOperationException("Cannot start value scope inside of existing value scope");
    }

    public ITestInspectAwaiter GetAwaiter()
    {
        return _parent.GetAwaiter();
    }
}