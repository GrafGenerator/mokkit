using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mokkit.Inspect;

internal class TestInspectScopeWithContext<T, TContext> : ITestInspectScopeWithContext<T, TContext>
{
    private readonly List<InspectValueWithContextAsyncFn<T, TContext>> _innerFns;
    private readonly TestInspect _parent;

    public TestInspectScopeWithContext(List<InspectValueWithContextAsyncFn<T, TContext>> innerFns, TestInspect parent)
    {
        _innerFns = innerFns;
        _parent = parent;
    }

    public ITestInspectScopeWithContext<T, TContext> Then(InspectValueWithContextAsyncFn<T, TContext> inspectFn)
    {
        _innerFns.Add(inspectFn);
        return this;
    }

    public ITestInspectScopeWithContext<T, TContext> Then(InspectValueWithContextFn<T, TContext> inspectFn)
    {
        _innerFns.Add((value, context, host) =>
        {
            inspectFn(value, context, host);
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

    public ITestInspectScopeWithContext<T1, TContext1> ThenValueScope<T1, TContext1>(T1 value, TContext1 context,
        InspectScopeAsyncFn? inspectScopeFn = null)
    {
        throw new InvalidOperationException("Cannot start value scope inside of existing value scope");
    }

    public ITestInspectAwaiter GetAwaiter()
    {
        return _parent.GetAwaiter();
    }
}