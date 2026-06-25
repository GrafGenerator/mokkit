using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mokkit.Containers;

namespace Mokkit.Suite;

/// <summary>
/// Represents a concrete implementation of <see cref="ITestHost"/> that provides dependency injection and service execution capabilities for test scenarios.
/// This class manages the lifecycle of dependency container scopes and executes actions with resolved services from those containers.
/// </summary>
public class TestHost : ITestHost
{
    private readonly ITestHostBagAccessor _bagAccessor;
    private readonly ScopeAggregator _scope;
    private readonly TestHostBag _bag;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestHost"/> class with the specified containers, bag accessor, and test host identifier.
    /// This constructor creates a new test host bag, establishes the test host context, and initializes the scope aggregator for dependency resolution.
    /// </summary>
    /// <param name="containers">The collection of dependency containers to aggregate for service resolution.</param>
    /// <param name="bagAccessor">The test host bag accessor for managing shared test resources.</param>
    /// <param name="testHostId">The unique identifier for this test host instance.</param>
    protected TestHost(IEnumerable<IDependencyContainer> containers, ITestHostBagAccessor bagAccessor, Guid testHostId)
    {
        _bag = new TestHostBag();
        _bagAccessor = bagAccessor;

        var context = new TestHostContext(testHostId, _bag);
        _scope = new ScopeAggregator(containers.ToArray(), context);
    }

    /// <summary>
    /// Executes an action with a single resolved service of type <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve and pass to the action.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved service.</param>
    public void Execute<TService>(Action<TService> actionFn)
        where TService : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        actionFn(_scope.Resolve<TService>());
    }

    /// <summary>
    /// Executes an action with two resolved services of types <typeparamref name="TService"/> and <typeparamref name="TService2"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved services.</param>
    public void Execute<TService, TService2>(Action<TService, TService2> actionFn)
        where TService : class
        where TService2 : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>());
    }

    /// <summary>
    /// Executes an action with three resolved services of types <typeparamref name="TService"/>, <typeparamref name="TService2"/>, and <typeparamref name="TService3"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TService3">The type of the third service to resolve.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved services.</param>
    public void Execute<TService, TService2, TService3>(Action<TService, TService2, TService3> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>());
    }

    /// <summary>
    /// Executes an action with four resolved services of types <typeparamref name="TService"/>, <typeparamref name="TService2"/>, <typeparamref name="TService3"/>, and <typeparamref name="TService4"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TService3">The type of the third service to resolve.</typeparam>
    /// <typeparam name="TService4">The type of the fourth service to resolve.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved services.</param>
    public void Execute<TService, TService2, TService3, TService4>(
        Action<TService, TService2, TService3, TService4> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>(),
            _scope.Resolve<TService4>());
    }

    /// <summary>
    /// Executes a function with a single resolved service of type <typeparamref name="TService"/> and returns the result.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve and pass to the function.</typeparam>
    /// <typeparam name="TOutput">The type of the result returned by the function.</typeparam>
    /// <param name="actionFn">The function to execute with the resolved service.</param>
    /// <returns>The result of the function execution.</returns>
    public TOutput Execute<TService, TOutput>(Func<TService, TOutput> actionFn)
        where TService : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        return actionFn(_scope.Resolve<TService>());
    }

    /// <summary>
    /// Executes a function with two resolved services of types <typeparamref name="TService"/> and <typeparamref name="TService2"/> and returns the result.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TOutput">The type of the result returned by the function.</typeparam>
    /// <param name="actionFn">The function to execute with the resolved services.</param>
    /// <returns>The result of the function execution.</returns>
    public TOutput Execute<TService, TService2, TOutput>(Func<TService, TService2, TOutput> actionFn)
        where TService : class
        where TService2 : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        return actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>());
    }

    /// <summary>
    /// Executes a function with three resolved services of types <typeparamref name="TService"/>, <typeparamref name="TService2"/>, and <typeparamref name="TService3"/> and returns the result.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TService3">The type of the third service to resolve.</typeparam>
    /// <typeparam name="TOutput">The type of the result returned by the function.</typeparam>
    /// <param name="actionFn">The function to execute with the resolved services.</param>
    /// <returns>The result of the function execution.</returns>
    public TOutput Execute<TService, TService2, TService3, TOutput>(
        Func<TService, TService2, TService3, TOutput> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        return actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>());
    }

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
    public TOutput Execute<TService, TService2, TService3, TService4, TOutput>(
        Func<TService, TService2, TService3, TService4, TOutput> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        return actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>(),
            _scope.Resolve<TService4>());
    }

    /// <summary>
    /// Executes an action with a single resolved service of type <typeparamref name="TService"/> asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve and pass to the action.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved service.</param>
    public async Task ExecuteAsync<TService>(Func<TService, Task> actionFn)
        where TService : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        await actionFn(_scope.Resolve<TService>());
    }

    /// <summary>
    /// Executes an action with two resolved services of types <typeparamref name="TService"/> and <typeparamref name="TService2"/> asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved services.</param>
    public async Task ExecuteAsync<TService, TService2>(Func<TService, TService2, Task> actionFn)
        where TService : class
        where TService2 : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        await actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>());
    }

    /// <summary>
    /// Executes an action with three resolved services of types <typeparamref name="TService"/>, <typeparamref name="TService2"/>, and <typeparamref name="TService3"/> asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TService3">The type of the third service to resolve.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved services.</param>
    public async Task ExecuteAsync<TService, TService2, TService3>(Func<TService, TService2, TService3, Task> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        await actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>());
    }

    /// <summary>
    /// Executes an action with four resolved services of types <typeparamref name="TService"/>, <typeparamref name="TService2"/>, <typeparamref name="TService3"/>, and <typeparamref name="TService4"/> asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TService3">The type of the third service to resolve.</typeparam>
    /// <typeparam name="TService4">The type of the fourth service to resolve.</typeparam>
    /// <param name="actionFn">The action to execute with the resolved services.</param>
    public async Task ExecuteAsync<TService, TService2, TService3, TService4>(
        Func<TService, TService2, TService3, TService4, Task> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        await actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>(),
            _scope.Resolve<TService4>());
    }

    /// <summary>
    /// Executes a function with a single resolved service of type <typeparamref name="TService"/> and returns the result asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve and pass to the function.</typeparam>
    /// <typeparam name="TOutput">The type of the result returned by the function.</typeparam>
    /// <param name="actionFn">The function to execute with the resolved service.</param>
    /// <returns>The result of the function execution.</returns>
    public async Task<TOutput> ExecuteAsync<TService, TOutput>(Func<TService, Task<TOutput>> actionFn)
        where TService : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        return await actionFn(_scope.Resolve<TService>());
    }

    /// <summary>
    /// Executes a function with two resolved services of types <typeparamref name="TService"/> and <typeparamref name="TService2"/> and returns the result asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TOutput">The type of the result returned by the function.</typeparam>
    /// <param name="actionFn">The function to execute with the resolved services.</param>
    /// <returns>The result of the function execution.</returns>
    public async Task<TOutput> ExecuteAsync<TService, TService2, TOutput>(
        Func<TService, TService2, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        return await actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>());
    }

    /// <summary>
    /// Executes a function with three resolved services of types <typeparamref name="TService"/>, <typeparamref name="TService2"/>, and <typeparamref name="TService3"/> and returns the result asynchronously.
    /// </summary>
    /// <typeparam name="TService">The type of the first service to resolve.</typeparam>
    /// <typeparam name="TService2">The type of the second service to resolve.</typeparam>
    /// <typeparam name="TService3">The type of the third service to resolve.</typeparam>
    /// <typeparam name="TOutput">The type of the result returned by the function.</typeparam>
    /// <param name="actionFn">The function to execute with the resolved services.</param>
    /// <returns>The result of the function execution.</returns>
    public async Task<TOutput> ExecuteAsync<TService, TService2, TService3, TOutput>(
        Func<TService, TService2, TService3, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        return await actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>());
    }

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
    public async Task<TOutput> ExecuteAsync<TService, TService2, TService3, TService4, TOutput>(
        Func<TService, TService2, TService3, TService4, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
    {
        _bagAccessor.Bag = _bag;
        _scope.OnAsyncScopeEnter();

        return await actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>(),
            _scope.Resolve<TService4>());
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}