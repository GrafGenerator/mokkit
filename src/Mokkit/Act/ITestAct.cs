using System;
using System.Threading.Tasks;
using Mokkit.Suite;

namespace Mokkit.Act;

/// <summary>
/// Represents the fluent interface for chaining act operations in the AAA (Arrange-Act-Assert) pattern.
/// Act is a first-class phase, symmetric with <see cref="Mokkit.Arrange.ITestArrange"/> and
/// <see cref="Mokkit.Inspect.ITestInspect"/>: it performs the operation(s) under test and — like Arrange —
/// may produce artifacts (through captures or a returned result) that later phases observe.
/// </summary>
public interface ITestAct
{
    /// <summary>
    /// Chains an asynchronous act function to be executed in sequence.
    /// </summary>
    /// <param name="actFn">The asynchronous act function to execute.</param>
    /// <returns>The current <see cref="ITestAct"/> instance for method chaining.</returns>
    ITestAct Then(ActAsyncFn actFn);

    /// <summary>
    /// Chains a synchronous act function to be executed in sequence.
    /// </summary>
    /// <param name="actFn">The synchronous act function to execute.</param>
    /// <returns>The current <see cref="ITestAct"/> instance for method chaining.</returns>
    ITestAct Then(ActFn actFn);

    /// <summary>
    /// Runs the pending act steps and then a producer that yields a result, turning the chain into a
    /// result-bearing act. This backs the "return" vocabulary flavor (<c>var r = await Act.DoThing(...)</c>),
    /// where an act operation returns the artifact directly instead of capturing it through an out parameter.
    /// </summary>
    /// <typeparam name="T">The type of the produced result.</typeparam>
    /// <param name="producerFn">The asynchronous function that performs the operation and returns its result.</param>
    /// <returns>A result-bearing act that can be awaited to yield the produced value.</returns>
    ITestAct<T> Returning<T>(Func<ITestHost, Task<T>> producerFn);

    /// <summary>
    /// Runs the pending act steps and then a synchronous producer that yields a result.
    /// </summary>
    /// <typeparam name="T">The type of the produced result.</typeparam>
    /// <param name="producerFn">The synchronous function that performs the operation and returns its result.</param>
    /// <returns>A result-bearing act that can be awaited to yield the produced value.</returns>
    ITestAct<T> Returning<T>(Func<ITestHost, T> producerFn);

    /// <summary>
    /// Gets an awaiter that allows the act chain to be awaited using the async/await pattern.
    /// This enables the use of 'await' directly on the act chain.
    /// </summary>
    /// <returns>An awaiter for the act operation chain.</returns>
    ITestActAwaiter GetAwaiter();
}

/// <summary>
/// Represents a result-bearing act — an act chain whose final operation produces a value of type
/// <typeparamref name="T"/>. Awaiting it runs the pending steps and yields that value, so an act operation
/// can return its artifact directly (<c>var result = await Act.SaveClient(command);</c>).
/// </summary>
/// <typeparam name="T">The type of the produced result.</typeparam>
public interface ITestAct<out T>
{
    /// <summary>
    /// Gets an awaiter that runs the act and yields its produced result via the async/await pattern.
    /// </summary>
    /// <returns>An awaiter for the result-bearing act.</returns>
    ITestActAwaiter<T> GetAwaiter();
}
