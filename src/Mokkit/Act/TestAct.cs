using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mokkit.Suite;

namespace Mokkit.Act;

/// <summary>
/// Internal implementation of <see cref="ITestAct"/> that manages the execution of act functions in the AAA pattern.
/// This class collects and executes act functions sequentially during the test act phase.
/// </summary>
public class TestAct : ITestAct
{
    private readonly TestStage _stage;
    private readonly List<ActAsyncFn> _actFns = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="TestAct"/> class with a test stage.
    /// This constructor is used when creating an act instance from a test stage context.
    /// </summary>
    /// <param name="stage">The test stage that provides the execution context.</param>
    internal TestAct(TestStage stage)
    {
        _stage = stage;
    }

    /// <summary>
    /// Adds an act function to the act instance.
    /// </summary>
    /// <param name="actFn">The act function to add to the act instance.</param>
    /// <returns>The act instance for method chaining.</returns>
    public ITestAct Then(ActAsyncFn actFn)
    {
        _actFns.Add(actFn);
        return this;
    }

    /// <summary>
    /// Adds an act function to the act instance.
    /// </summary>
    /// <param name="actFn">The act function to add to the act instance.</param>
    /// <returns>The act instance for method chaining.</returns>
    public ITestAct Then(ActFn actFn)
    {
        _actFns.Add(host =>
        {
            actFn(host);
            return Task.CompletedTask;
        });

        return this;
    }

    /// <summary>
    /// Turns the pending act steps into a result-bearing act by appending a producer that yields a value.
    /// </summary>
    /// <typeparam name="T">The type of the produced result.</typeparam>
    /// <param name="producerFn">The asynchronous function that performs the operation and returns its result.</param>
    /// <returns>A <see cref="TestAct{T}"/> that runs the pending steps then the producer.</returns>
    public ITestAct<T> Returning<T>(Func<ITestHost, Task<T>> producerFn)
    {
        return new TestAct<T>(_stage, _actFns, producerFn);
    }

    /// <summary>
    /// Turns the pending act steps into a result-bearing act by appending a synchronous producer.
    /// </summary>
    /// <typeparam name="T">The type of the produced result.</typeparam>
    /// <param name="producerFn">The synchronous function that performs the operation and returns its result.</param>
    /// <returns>A <see cref="TestAct{T}"/> that runs the pending steps then the producer.</returns>
    public ITestAct<T> Returning<T>(Func<ITestHost, T> producerFn)
    {
        return new TestAct<T>(_stage, _actFns, host => Task.FromResult(producerFn(host)));
    }

    /// <summary>
    /// Executes all registered act functions sequentially in the order they were added.
    /// This method is called internally during the test execution lifecycle to perform the act phase.
    /// </summary>
    /// <returns>A task that represents the asynchronous act operation.</returns>
    internal async Task DoActAsync()
    {
        foreach (var actFn in _actFns)
        {
            await actFn(_stage);
        }
    }

    /// <summary>
    /// Gets an awaiter for the act instance.
    /// </summary>
    /// <returns>The awaiter for the act instance.</returns>
    public ITestActAwaiter GetAwaiter()
    {
        return new TestActAwaiter(this);
    }
}

/// <summary>
/// Internal implementation of <see cref="ITestAct{T}"/> — a result-bearing act that runs its pending steps
/// and then a producer, yielding the produced value when awaited.
/// </summary>
/// <typeparam name="T">The type of the produced result.</typeparam>
public class TestAct<T> : ITestAct<T>
{
    private readonly TestStage _stage;
    private readonly List<ActAsyncFn> _actFns;
    private readonly Func<ITestHost, Task<T>> _producerFn;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestAct{T}"/> class.
    /// </summary>
    /// <param name="stage">The test stage that provides the execution context.</param>
    /// <param name="actFns">The pending act steps to run before the producer.</param>
    /// <param name="producerFn">The producer that performs the final operation and returns its result.</param>
    internal TestAct(TestStage stage, List<ActAsyncFn> actFns, Func<ITestHost, Task<T>> producerFn)
    {
        _stage = stage;
        _actFns = actFns;
        _producerFn = producerFn;
    }

    /// <summary>
    /// Executes the pending act steps in order, then the producer, and returns its result.
    /// </summary>
    /// <returns>A task that yields the value produced by the act.</returns>
    internal async Task<T> DoActAsync()
    {
        foreach (var actFn in _actFns)
        {
            await actFn(_stage);
        }

        return await _producerFn(_stage);
    }

    /// <summary>
    /// Gets an awaiter for the result-bearing act instance.
    /// </summary>
    /// <returns>The awaiter for the result-bearing act instance.</returns>
    public ITestActAwaiter<T> GetAwaiter()
    {
        return new TestActAwaiter<T>(this);
    }
}
