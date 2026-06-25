using System.Collections.Generic;
using System.Threading.Tasks;
using Mokkit.Suite;

namespace Mokkit.Arrange;

/// <summary>
/// Internal implementation of <see cref="ITestArrange"/> that manages the execution of arrange functions in the AAA pattern.
/// This class collects and executes arrange functions sequentially during the test arrangement phase.
/// </summary>
public class TestArrange : ITestArrange
{
    private readonly TestStage _stage;
    private readonly List<ArrangeAsyncFn> _arrangeFns = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="TestArrange"/> class with a test stage.
    /// This constructor is used when creating an arrange instance from a test stage context.
    /// </summary>
    /// <param name="stage">The test stage that provides the execution context.</param>
    internal TestArrange(TestStage stage)
    {
        _stage = stage;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestArrange"/> class with a synchronous arrange function.
    /// The synchronous function is wrapped in an async delegate for uniform execution.
    /// </summary>
    /// <param name="arrangeFn">The synchronous arrange function to execute.</param>
    internal TestArrange(ArrangeFn arrangeFn)
    {
        _arrangeFns.Add(host =>
        {
            arrangeFn(host);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestArrange"/> class with an asynchronous arrange function.
    /// </summary>
    /// <param name="arrangeFn">The asynchronous arrange function to execute.</param>
    internal TestArrange(ArrangeAsyncFn arrangeFn)
    {
        _arrangeFns.Add(arrangeFn);
    }

    /// <summary>
    /// Adds an arrange function to the arrange instance.
    /// </summary>
    /// <param name="arrangeFn">The arrange function to add to the arrange instance.</param>
    /// <returns>The arrange instance for method chaining.</returns>
    public ITestArrange Then(ArrangeAsyncFn arrangeFn)
    {
        _arrangeFns.Add(arrangeFn);
        return this;
    }

    /// <summary>
    /// Adds an arrange function to the arrange instance.
    /// </summary>
    /// <param name="arrangeFn">The arrange function to add to the arrange instance.</param>
    /// <returns>The arrange instance for method chaining.</returns>
    public ITestArrange Then(ArrangeFn arrangeFn)
    {
        _arrangeFns.Add(host =>
        {
            arrangeFn(host);
            return Task.CompletedTask;
        });

        return this;
    }

    /// <summary>
    /// Executes all registered arrange functions sequentially in the order they were added.
    /// This method is called internally during the test execution lifecycle to perform the arrangement phase.
    /// </summary>
    /// <returns>A task that represents the asynchronous arrange operation.</returns>
    internal async Task DoArrangeAsync()
    {
        foreach (var arrangeFn in _arrangeFns)
        {
            await arrangeFn(_stage);
        }
    }

    /// <summary>
    /// Gets an awaiter for the arrange instance.
    /// </summary>
    /// <returns>The awaiter for the arrange instance.</returns>
    public ITestArrangeAwaiter GetAwaiter()
    {
        return new TestArrangeAwaiter(this);
    }
}