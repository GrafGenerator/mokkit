using System;
using System.Threading.Tasks;

namespace Mokkit.Suite;

public interface ITestHost: IDisposable
{
    void Execute<TService>(Action<TService> actionFn)
        where TService : class;

    void Execute<TService, TService2>(Action<TService, TService2> actionFn)
        where TService : class
        where TService2 : class;

    TOutput Execute<TService, TOutput>(Func<TService, TOutput> actionFn)
        where TService : class;

    Task ExecuteAsync<TService>(Func<TService, Task> actionFn)
        where TService : class;

    Task<TOutput> ExecuteAsync<TService, TOutput>(Func<TService, Task<TOutput>> actionFn)
        where TService : class;
}