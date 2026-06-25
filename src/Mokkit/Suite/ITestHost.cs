using System;
using System.Threading.Tasks;

namespace Mokkit.Suite;

/// <summary>
/// Represents a test host that provides dependency injection and service execution capabilities for test scenarios.
/// This interface extends <see cref="IDisposable"/> to ensure proper cleanup of resources after test execution.
/// The test host resolves services from configured dependency containers and executes actions with those services.
/// </summary>
public interface ITestHost : IDisposable
{
    /// <summary>
    /// Executes an action with a single resolved service of type <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve and pass to the action.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved service.</param>
    void Execute<TService>(Action<TService> actionFn)
        where TService : class;

    /// <summary>
    /// Executes an action with two resolved services of types <typeparamref name="TService"/> and <typeparamref name="TService2"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved services.</param>
    void Execute<TService, TService2>(Action<TService, TService2> actionFn)
        where TService : class
        where TService2 : class;

    /// <summary>
    /// Executes an action with three resolved services of types <typeparamref name="TService"/>, <typeparamref name="TService2"/>, and <typeparamref name="TService3"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TService3">The type of the third service to resolve.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved services.</param>
    void Execute<TService, TService2, TService3>(Action<TService, TService2, TService3> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class;

    /// <summary>
    /// Executes an action with four resolved services of types <typeparamref name="TService"/>, <typeparamref name="TService2"/>, <typeparamref name="TService3"/>, and <typeparamref name="TService4"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TService3">The type of the third service to resolve.</typeparam>
    /// <typeparam name="TService4">The type of the fourth service to resolve.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved services.</param>
    void Execute<TService, TService2, TService3, TService4>(
        Action<TService, TService2, TService3, TService4> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class;

    /// <summary>
    /// Executes a function with a single resolved service of type <typeparamref name="TService"/> and returns the result.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve and pass to the function.</typeparam>
    /// <typeparam name="TOutput">The type of the result returned by the function.</typeparam>
    /// <param name="actionFn">The function to execute with the resolved service.</param>
    /// <returns>The result of the function execution.</returns>
    TOutput Execute<TService, TOutput>(Func<TService, TOutput> actionFn)
        where TService : class;

    /// <summary>
    /// Executes a function with two resolved services of types <typeparamref name="TService"/> and <typeparamref name="TService2"/> and returns the result.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TOutput">The type of the result returned by the function.</typeparam>
    /// <param name="actionFn">The function to execute with the resolved services.</param>
    /// <returns>The result of the function execution.</returns>
    TOutput Execute<TService, TService2, TOutput>(Func<TService, TService2, TOutput> actionFn)
        where TService : class
        where TService2 : class;

    /// <summary>
    /// Executes a function with three resolved services of types <typeparamref name="TService"/>, <typeparamref name="TService2"/>, and <typeparamref name="TService3"/> and returns the result.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TService3">The type of the third service to resolve.</typeparam>
    /// <typeparam name="TOutput">The type of the result returned by the function.</typeparam>
    /// <param name="actionFn">The function to execute with the resolved services.</param>
    /// <returns>The result of the function execution.</returns>
    TOutput Execute<TService, TService2, TService3, TOutput>(
        Func<TService, TService2, TService3, TOutput> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class;

    /// <summary>
    /// Executes a function with four resolved services of types <typeparamref name="TService"/>, <typeparamref name="TService2"/>, <typeparamref name="TService3"/>, and <typeparamref name="TService4"/> and returns the result.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TService3">The type of the third service to resolve.</typeparam>
    /// <typeparam name="TService4">The type of the fourth service to resolve.</typeparam>
    /// <typeparam name="TOutput">The type of the result returned by the function.</typeparam>
    /// <param name="actionFn">The function to execute with the resolved services.</param>
    /// <returns>The result of the function execution.</returns>
    TOutput Execute<TService, TService2, TService3, TService4, TOutput>(
        Func<TService, TService2, TService3, TService4, TOutput> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class;

    /// <summary>
    /// Executes an action with a single resolved service of type <typeparamref name="TService"/> asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve and pass to the action.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved service.</param>
    Task ExecuteAsync<TService>(Func<TService, Task> actionFn)
        where TService : class;

    /// <summary>
    /// Executes an action with two resolved services of types <typeparamref name="TService"/> and <typeparamref name="TService2"/> asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved services.</param>
    Task ExecuteAsync<TService, TService2>(Func<TService, TService2, Task> actionFn)
        where TService : class
        where TService2 : class;

    /// <summary>
    /// Executes an action with three resolved services of types <typeparamref name="TService"/>, <typeparamref name="TService2"/>, and <typeparamref name="TService3"/> asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TService3">The type of the third service to resolve.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved services.</param>
    Task ExecuteAsync<TService, TService2, TService3>(Func<TService, TService2, TService3, Task> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class;

    /// <summary>
    /// Executes an action with four resolved services of types <typeparamref name="TService"/>, <typeparamref name="TService2"/>, <typeparamref name="TService3"/>, and <typeparamref name="TService4"/> asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TService3">The type of the third service to resolve.</typeparam>
    /// <typeparam name="TService4">The type of the fourth service to resolve.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved services.</param>
    Task ExecuteAsync<TService, TService2, TService3, TService4>(
        Func<TService, TService2, TService3, TService4, Task> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class;

    /// <summary>
    /// Executes a function with a single resolved service of type <typeparamref name="TService"/> and returns the result asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve and pass to the function.</typeparam>
    /// <typeparam name="TOutput">The type of the result returned by the function.</typeparam>
    /// <param name="actionFn">The function to execute with the resolved service.</param>
    /// <returns>The result of the function execution.</returns>
    Task<TOutput> ExecuteAsync<TService, TOutput>(Func<TService, Task<TOutput>> actionFn)
        where TService : class;

    /// <summary>
    /// Executes a function with two resolved services of types <typeparamref name="TService"/> and <typeparamref name="TService2"/> and returns the result asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TOutput">The type of the result returned by the function.</typeparam>
    /// <param name="actionFn">The function to execute with the resolved services.</param>
    /// <returns>The result of the function execution.</returns>
    Task<TOutput> ExecuteAsync<TService, TService2, TOutput>(
        Func<TService, TService2, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class;

    /// <summary>
    /// Executes a function with three resolved services of types <typeparamref name="TService"/>, <typeparamref name="TService2"/>, and <typeparamref name="TService3"/> and returns the result asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TService3">The type of the third service to resolve.</typeparam>
    /// <typeparam name="TOutput">The type of the result returned by the function.</typeparam>
    /// <param name="actionFn">The function to execute with the resolved services.</param>
    /// <returns>The result of the function execution.</returns>
    Task<TOutput> ExecuteAsync<TService, TService2, TService3, TOutput>(
        Func<TService, TService2, TService3, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class;

    /// <summary>
    /// Executes a function with four resolved services of types <typeparamref name="TService"/>, <typeparamref name="TService2"/>, <typeparamref name="TService3"/>, and <typeparamref name="TService4"/> and returns the result asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TService3">The type of the third service to resolve.</typeparam>
    /// <typeparam name="TService4">The type of the fourth service to resolve.</typeparam>
    /// <typeparam name="TOutput">The type of the result returned by the function.</typeparam>
    /// <param name="actionFn">The function to execute with the resolved services.</param>
    /// <returns>The result of the function execution.</returns>
    Task<TOutput> ExecuteAsync<TService, TService2, TService3, TService4, TOutput>(
        Func<TService, TService2, TService3, TService4, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class;
}