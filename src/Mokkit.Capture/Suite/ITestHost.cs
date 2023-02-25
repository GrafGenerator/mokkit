using System;
using System.Threading.Tasks;

namespace Mokkit.Capture.Suite;

public interface ITestHost
{
    void ExecuteAsync<TService>(Action<TService> actionFn);
    
    void ExecuteAsync<TService, TService2>(Action<TService, TService2> actionFn);

    TOutput ExecuteAsync<TService, TOutput>(Func<TService, TOutput> actionFn);
    
    Task ExecuteAsync<TService>(Func<TService, Task> actionFn);

    Task<TOutput> ExecuteAsync<TService, TOutput>(Func<TService, Task<TOutput>> actionFn);
}