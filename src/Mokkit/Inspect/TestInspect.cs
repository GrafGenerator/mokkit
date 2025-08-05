using System.Collections.Generic;
using System.Threading.Tasks;
using Mokkit.Suite;

namespace Mokkit.Inspect;

/// <summary>
/// Internal implementation of <see cref="ITestInspect"/> that manages the execution of inspect functions in the AAA pattern.
/// This class collects and executes inspect functions sequentially during the test assertion/verification phase.
/// </summary>
internal class TestInspect : ITestInspect
{
    private readonly TestStage _stage;
    private readonly List<InspectAsyncFn> _inspectFns = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TestInspect"/> class with a test stage.
    /// This constructor is used when creating an inspect instance from a test stage context.
    /// </summary>
    /// <param name="stage">The test stage that provides the execution context.</param>
    internal TestInspect(TestStage stage)
    {
        _stage = stage;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TestInspect"/> class with a synchronous inspect function.
    /// The synchronous function is wrapped in an async delegate for uniform execution.
    /// </summary>
    /// <param name="inspectFn">The synchronous inspect function to execute.</param>
    internal TestInspect(InspectFn inspectFn)
    {
        _inspectFns.Add(host =>
        {
            inspectFn(host);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestInspect"/> class with an asynchronous inspect function.
    /// </summary>
    /// <param name="inspectFn">The asynchronous inspect function to execute.</param>
    internal TestInspect(InspectAsyncFn inspectFn)
    {
        _inspectFns.Add(inspectFn);
    }

    /// <summary>
    /// Adds an asynchronous inspect function to the execution pipeline.
    /// </summary>
    /// <param name="inspectFn">The asynchronous inspect function to add.</param>
    /// <returns>The current <see cref="ITestInspect"/> instance for fluent chaining.</returns>
    public ITestInspect Then(InspectAsyncFn inspectFn)
    {
        _inspectFns.Add(inspectFn);
        return this;
    }
    
    /// <summary>
    /// Adds a synchronous inspect function to the execution pipeline.
    /// The synchronous function is wrapped in an async delegate for uniform execution.
    /// </summary>
    /// <param name="inspectFn">The synchronous inspect function to add.</param>
    /// <returns>The current <see cref="ITestInspect"/> instance for fluent chaining.</returns>
    public ITestInspect Then(InspectFn inspectFn)
    {
        _inspectFns.Add(host =>
        { 
            inspectFn(host);
            return Task.CompletedTask;
        });
        
        return this;
    }

    /// <summary>
    /// Creates a new scope for inspecting a value with an optional scope function.
    /// </summary>
    /// <typeparam name="T">The type of the value to inspect.</typeparam>
    /// <param name="value">The value to inspect.</param>
    /// <param name="inspectScopeFn">The optional scope function to execute.</param>
    /// <returns>A new <see cref="ITestInspectScope{T}"/> instance for the value.</returns>
    public ITestInspectScope<T> ThenValueScope<T>(T value, InspectScopeAsyncFn? inspectScopeFn = null)
    {
        var innerFns = new List<InspectValueAsyncFn<T>>();
        
        var scopeFn = inspectScopeFn ?? (async (_, executeInnerFns) =>
        {
            await executeInnerFns();
        });
        
        _inspectFns.Add(host => scopeFn(host, ExecuteInnerFns));
        
        return new TestInspectScope<T>(innerFns, this);

        async Task ExecuteInnerFns()
        {
            foreach (var innerFn in innerFns)
            {
                await innerFn(value, _stage);
            }
        }
    }
    
    /// <summary>
    /// Creates a new scope for inspecting a value with a context and an optional scope function.
    /// </summary>
    /// <typeparam name="T">The type of the value to inspect.</typeparam>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <param name="value">The value to inspect.</param>
    /// <param name="context">The context for the inspection.</param>
    /// <param name="inspectScopeFn">The optional scope function to execute.</param>
    /// <returns>A new <see cref="ITestInspectScopeWithContext{T, TContext}"/> instance for the value and context.</returns>
    public ITestInspectScopeWithContext<T, TContext> ThenValueScope<T, TContext>(T value, TContext context, InspectScopeAsyncFn? inspectScopeFn = null)
    {
        var innerFns = new List<InspectValueWithContextAsyncFn<T, TContext>>();
        
        var scopeFn = inspectScopeFn ?? (async (_, executeInnerFns) =>
        {
            await executeInnerFns();
        });
        
        _inspectFns.Add(host => scopeFn(host, ExecuteInnerFns));
        
        return new TestInspectScopeWithContext<T, TContext>(innerFns, this);

        async Task ExecuteInnerFns()
        {
            foreach (var innerFn in innerFns)
            {
                await innerFn(value, context, _stage);
            }
        }
    }
    
    /// <summary>
    /// Executes all registered inspect functions sequentially in the order they were added.
    /// This method is called internally during the test execution lifecycle to perform the assertion/verification phase.
    /// </summary>
    /// <returns>A task that represents the asynchronous inspect operation.</returns>
    internal async Task DoInspectAsync()
    {
        foreach (var inspectFn in _inspectFns)
        {
            await inspectFn(_stage);
        }
    }
    
    /// <summary>
    /// Gets an awaiter for the inspect operation.
    /// </summary>
    /// <returns>An awaiter for the inspect operation.</returns>
    public ITestInspectAwaiter GetAwaiter()
    {
        return new TestInspectAwaiter(this);
    }
}