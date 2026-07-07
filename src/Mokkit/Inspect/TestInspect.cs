using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mokkit.Suite;

namespace Mokkit.Inspect;

/// <summary>
/// Internal implementation of <see cref="ITestInspect"/> that manages the execution of inspect functions in the AAA pattern.
/// Inspect steps are collected as ordered groups: sequential steps (added via <see cref="Then(InspectAsyncFn)"/>) run one
/// after another, while a group added via <see cref="ThenAll(InspectAsyncFn[])"/> runs its functions concurrently. Groups
/// always execute in the order they were added, so <c>Then</c>/<c>ThenAll</c> ordering is preserved.
/// </summary>
internal class TestInspect : ITestInspect
{
    private readonly TestStage _stage;
    private readonly List<StepGroup> _groups = new();

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
        AddSequential(Wrap(inspectFn));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestInspect"/> class with an asynchronous inspect function.
    /// </summary>
    /// <param name="inspectFn">The asynchronous inspect function to execute.</param>
    internal TestInspect(InspectAsyncFn inspectFn)
    {
        AddSequential(inspectFn);
    }

    /// <summary>
    /// Adds an asynchronous inspect function to the execution pipeline as a sequential step.
    /// </summary>
    /// <param name="inspectFn">The asynchronous inspect function to add.</param>
    /// <returns>The current <see cref="ITestInspect"/> instance for fluent chaining.</returns>
    public ITestInspect Then(InspectAsyncFn inspectFn)
    {
        AddSequential(inspectFn);
        return this;
    }

    /// <summary>
    /// Adds a synchronous inspect function to the execution pipeline as a sequential step.
    /// The synchronous function is wrapped in an async delegate for uniform execution.
    /// </summary>
    /// <param name="inspectFn">The synchronous inspect function to add.</param>
    /// <returns>The current <see cref="ITestInspect"/> instance for fluent chaining.</returns>
    public ITestInspect Then(InspectFn inspectFn)
    {
        AddSequential(Wrap(inspectFn));
        return this;
    }

    /// <summary>
    /// Adds a group of asynchronous inspect functions that execute <b>concurrently</b> with one another.
    /// Steps added before and after this group still run sequentially — only the functions within this single call overlap.
    /// </summary>
    /// <param name="inspectFns">The asynchronous inspect functions to run in parallel.</param>
    /// <returns>The current <see cref="ITestInspect"/> instance for fluent chaining.</returns>
    public ITestInspect ThenAll(params InspectAsyncFn[] inspectFns)
    {
        _groups.Add(new StepGroup(true, inspectFns));
        return this;
    }

    /// <summary>
    /// Adds a group of synchronous inspect functions that execute <b>concurrently</b> with one another.
    /// Each function is wrapped in an async delegate for uniform execution.
    /// </summary>
    /// <param name="inspectFns">The synchronous inspect functions to run in parallel.</param>
    /// <returns>The current <see cref="ITestInspect"/> instance for fluent chaining.</returns>
    public ITestInspect ThenAll(params InspectFn[] inspectFns)
    {
        _groups.Add(new StepGroup(true, Array.ConvertAll(inspectFns, Wrap)));
        return this;
    }

    /// <summary>
    /// Adds a group of inspect branches that execute <b>concurrently</b> with one another, where each branch is built
    /// with the same fluent inspect helpers (e.g. <c>b =&gt; b.ApiClient(id, ...).DbClient(id, ...)</c>). Steps within a
    /// branch run sequentially; the branches themselves overlap. This is the way to parallelize a chain that is composed
    /// from extension-method helpers rather than raw host lambdas.
    /// </summary>
    /// <param name="branches">The branch builders; each receives a fresh sub-chain bound to the same stage.</param>
    /// <returns>The current <see cref="ITestInspect"/> instance for fluent chaining.</returns>
    public ITestInspect ThenAll(params Func<ITestInspect, ITestInspect>[] branches)
    {
        var branchFns = new InspectAsyncFn[branches.Length];

        for (var i = 0; i < branches.Length; i++)
        {
            var branch = new TestInspect(_stage);
            branches[i](branch);
            branchFns[i] = _ => branch.DoInspectAsync();
        }

        _groups.Add(new StepGroup(true, branchFns));
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

        AddSequential(host => scopeFn(host, ExecuteInnerFns));

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

        AddSequential(host => scopeFn(host, ExecuteInnerFns));

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
    /// Executes all registered inspect groups in the order they were added.
    /// Sequential groups run their function inline; parallel groups run their functions concurrently via <see cref="Task.WhenAll(System.Collections.Generic.IEnumerable{Task})"/>.
    /// The scope/bag is established once up front so concurrent steps resolve services safely.
    /// </summary>
    /// <returns>A task that represents the asynchronous inspect operation.</returns>
    internal async Task DoInspectAsync()
    {
        _stage?.PrepareForInspect();

        foreach (var group in _groups)
        {
            if (group.Parallel && group.Fns.Count > 1)
            {
                await Task.WhenAll(group.Fns.Select(fn => fn(_stage)));
            }
            else
            {
                foreach (var fn in group.Fns)
                {
                    await fn(_stage);
                }
            }
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

    private void AddSequential(InspectAsyncFn inspectFn)
    {
        _groups.Add(new StepGroup(false, new[] { inspectFn }));
    }

    private static InspectAsyncFn Wrap(InspectFn inspectFn)
    {
        return host =>
        {
            inspectFn(host);
            return Task.CompletedTask;
        };
    }

    /// <summary>
    /// An ordered inspect step: one or more functions that run either sequentially or, when <see cref="Parallel"/> is set, concurrently.
    /// </summary>
    private sealed class StepGroup
    {
        public StepGroup(bool parallel, IReadOnlyList<InspectAsyncFn> fns)
        {
            Parallel = parallel;
            Fns = fns;
        }

        public bool Parallel { get; }

        public IReadOnlyList<InspectAsyncFn> Fns { get; }
    }
}
