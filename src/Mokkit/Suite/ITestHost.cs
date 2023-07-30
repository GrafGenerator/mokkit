using System;
using System.Threading.Tasks;

namespace Mokkit.Capture.Suite;

public interface ITestHost
{
    void Execute<TService>(Action<TService> actionFn);
    
    void Execute<TService, TService2>(Action<TService, TService2> actionFn);

    TOutput Execute<TService, TOutput>(Func<TService, TOutput> actionFn);
    
    Task ExecuteAsync<TService>(Func<TService, Task> actionFn);

    Task<TOutput> ExecuteAsync<TService, TOutput>(Func<TService, Task<TOutput>> actionFn);
}