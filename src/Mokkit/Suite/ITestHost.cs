using System;
using System.Threading.Tasks;

namespace Mokkit.Suite;

public interface ITestHost : IDisposable
{
    void Execute<TService>(Action<TService> actionFn)
        where TService : class;

    void Execute<TService, TService2>(Action<TService, TService2> actionFn)
        where TService : class
        where TService2 : class;

    void Execute<TService, TService2, TService3>(Action<TService, TService2, TService3> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class;

    void Execute<TService, TService2, TService3, TService4>(
        Action<TService, TService2, TService3, TService4> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class;

    TOutput Execute<TService, TOutput>(Func<TService, TOutput> actionFn)
        where TService : class;

    TOutput Execute<TService, TService2, TOutput>(Func<TService, TService2, TOutput> actionFn)
        where TService : class
        where TService2 : class;

    TOutput Execute<TService, TService2, TService3, TOutput>(
        Func<TService, TService2, TService3, TOutput> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class;

    TOutput Execute<TService, TService2, TService3, TService4, TOutput>(
        Func<TService, TService2, TService3, TService4, TOutput> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class;

    Task ExecuteAsync<TService>(Func<TService, Task> actionFn)
        where TService : class;

    Task ExecuteAsync<TService, TService2>(Func<TService, TService2, Task> actionFn)
        where TService : class
        where TService2 : class;

    Task ExecuteAsync<TService, TService2, TService3>(Func<TService, TService2, TService3, Task> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class;

    Task ExecuteAsync<TService, TService2, TService3, TService4>(
        Func<TService, TService2, TService3, TService4, Task> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class;

    Task<TOutput> ExecuteAsync<TService, TOutput>(Func<TService, Task<TOutput>> actionFn)
        where TService : class;

    Task<TOutput> ExecuteAsync<TService, TService2, TOutput>(
        Func<TService, TService2, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class;

    Task<TOutput> ExecuteAsync<TService, TService2, TService3, TOutput>(
        Func<TService, TService2, TService3, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class;

    Task<TOutput> ExecuteAsync<TService, TService2, TService3, TService4, TOutput>(
        Func<TService, TService2, TService3, TService4, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class;
}