using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mokkit.Inspect;

/// <summary>
/// Internal implementation of <see cref="ITestInspectScopeWithContext{T, TContext}"/> that provides scoped inspection functionality for values of type <typeparamref name="T"/> with additional context of type <typeparamref name="TContext"/>.
/// This class manages the execution of value-specific inspect functions that require additional contextual information.
/// </summary>
/// <typeparam name="T">The type of value being inspected in this scope.</typeparam>
/// <typeparam name="TContext">The type of additional context provided to the inspect functions.</typeparam>
internal class TestInspectScopeWithContext<T, TContext> : ITestInspectScopeWithContext<T, TContext>
{
    private readonly List<InspectValueWithContextAsyncFn<T, TContext>> _innerFns;
    private readonly TestInspect _parent;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestInspectScopeWithContext{T, TContext}"/> class with the specified inner functions and parent inspect instance.
    /// </summary>
    /// <param name="innerFns">The list of inner inspect functions that accept both value and context parameters.</param>
    /// <param name="parent">The parent test inspect instance that created this scope.</param>
    public TestInspectScopeWithContext(List<InspectValueWithContextAsyncFn<T, TContext>> innerFns, TestInspect parent)
    {
        _innerFns = innerFns;
        _parent = parent;
    }

    /// <summary>
    /// Adds an inspect function that accepts both value and context parameters to the scope.
    /// </summary>
    /// <param name="inspectFn">The inspect function to add to the scope.</param>
    /// <returns>The current scope instance for method chaining.</returns>
    public ITestInspectScopeWithContext<T, TContext> Then(InspectValueWithContextAsyncFn<T, TContext> inspectFn)
    {
        _innerFns.Add(inspectFn);
        return this;
    }

    /// <summary>
    /// Adds an inspect function that accepts both value and context parameters to the scope.
    /// </summary>
    /// <param name="inspectFn">The inspect function to add to the scope.</param>
    /// <returns>The current scope instance for method chaining.</returns>
    public ITestInspectScopeWithContext<T, TContext> Then(InspectValueWithContextFn<T, TContext> inspectFn)
    {
        _innerFns.Add((value, context, host) =>
        {
            inspectFn(value, context, host);
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
    public ITestInspectScopeWithContext<T1, TContext1> ThenValueScope<T1, TContext1>(T1 value, TContext1 context,
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