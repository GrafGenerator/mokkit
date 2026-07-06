using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mokkit.Inspect;

/// <summary>
/// Internal implementation of <see cref="ITestInspectScope{T}"/> that provides scoped inspection functionality for values of type <typeparamref name="T"/>.
/// This class manages the execution of value-specific inspect functions within a scoped context.
/// </summary>
/// <typeparam name="T">The type of value being inspected in this scope.</typeparam>
internal class TestInspectScope<T> : ITestInspectScope<T>
{
    private readonly List<InspectValueAsyncFn<T>> _innerFns;
    private readonly TestInspect _parent;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestInspectScope{T}"/> class with the specified inner functions and parent inspect instance.
    /// </summary>
    /// <param name="innerFns">The list of inner inspect functions to execute within this scope.</param>
    /// <param name="parent">The parent test inspect instance that created this scope.</param>
    public TestInspectScope(List<InspectValueAsyncFn<T>> innerFns, TestInspect parent)
    {
        _innerFns = innerFns;
        _parent = parent;
    }

    /// <summary>
    /// Adds an inspect function to the scope.
    /// </summary>
    /// <param name="inspectFn">The inspect function to add to the scope.</param>
    /// <returns>The current scope instance for method chaining.</returns>
    public ITestInspectScope<T> Then(InspectValueAsyncFn<T> inspectFn)
    {
        _innerFns.Add(inspectFn);
        return this;
    }

    /// <summary>
    /// Adds an inspect function to the scope.
    /// </summary>
    /// <param name="inspectFn">The inspect function to add to the scope.</param>
    /// <returns>The current scope instance for method chaining.</returns>
    public ITestInspectScope<T> Then(InspectValueFn<T> inspectFn)
    {
        _innerFns.Add((value, host) =>
        {
            inspectFn(value, host);
            return Task.CompletedTask;
        });

        return this;
    }

    /// <summary>
    /// Adds an inspect function to the scope.
    /// </summary>
    /// <param name="inspectFn">The inspect function to add to the scope.</param>
    /// <returns>The current scope instance for method chaining.</returns>
    public ITestInspect Then(InspectAsyncFn inspectFn)
    {
        return _parent.Then(inspectFn);
    }

    /// <summary>
    /// Adds an inspect function to the scope.
    /// </summary>
    /// <param name="inspectFn">The inspect function to add to the scope.</param>
    /// <returns>The current scope instance for method chaining.</returns>
    public ITestInspect Then(InspectFn inspectFn)
    {
        return _parent.Then(inspectFn);
    }

    /// <summary>
    /// Adds a parallel inspect group by forwarding to the parent inspect chain.
    /// </summary>
    /// <param name="inspectFns">The asynchronous inspect functions to run in parallel.</param>
    /// <returns>The parent inspect instance for method chaining.</returns>
    public ITestInspect ThenAll(params InspectAsyncFn[] inspectFns)
    {
        return _parent.ThenAll(inspectFns);
    }

    /// <summary>
    /// Adds a parallel inspect group by forwarding to the parent inspect chain.
    /// </summary>
    /// <param name="inspectFns">The synchronous inspect functions to run in parallel.</param>
    /// <returns>The parent inspect instance for method chaining.</returns>
    public ITestInspect ThenAll(params InspectFn[] inspectFns)
    {
        return _parent.ThenAll(inspectFns);
    }

    /// <summary>
    /// Adds a parallel inspect branch group by forwarding to the parent inspect chain.
    /// </summary>
    /// <param name="branches">The branch builders to run in parallel.</param>
    /// <returns>The parent inspect instance for method chaining.</returns>
    public ITestInspect ThenAll(params Func<ITestInspect, ITestInspect>[] branches)
    {
        return _parent.ThenAll(branches);
    }

    /// <summary>
    /// Adds a value scope to the scope.
    /// </summary>
    /// <param name="value">The value to add to the scope.</param>
    /// <param name="inspectScopeFn">The inspect scope function to add to the scope.</param>
    /// <returns>The current scope instance for method chaining.</returns>
    public ITestInspectScope<T1> ThenValueScope<T1>(T1 value, InspectScopeAsyncFn? inspectScopeFn = null)
    {
        throw new InvalidOperationException("Cannot start value scope inside of existing value scope");
    }

    /// <summary>
    /// Adds a value scope to the scope.
    /// </summary>
    /// <param name="value">The value to add to the scope.</param>
    /// <param name="context">The context to add to the scope.</param>
    /// <param name="inspectScopeFn">The inspect scope function to add to the scope.</param>
    /// <returns>The current scope instance for method chaining.</returns>
    public ITestInspectScopeWithContext<T1, TContext> ThenValueScope<T1, TContext>(T1 value, TContext context,
        InspectScopeAsyncFn? inspectScopeFn = null)
    {
        throw new InvalidOperationException("Cannot start value scope inside of existing value scope");
    }

    /// <summary>
    /// Gets an awaiter for the scope.
    /// </summary>
    /// <returns>The awaiter for the scope.</returns>
    public ITestInspectAwaiter GetAwaiter()
    {
        return _parent.GetAwaiter();
    }
}