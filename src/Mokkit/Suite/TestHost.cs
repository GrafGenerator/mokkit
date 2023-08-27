using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mokkit.Containers;

namespace Mokkit.Suite;

public class TestHost : ITestHost
{
    private readonly TestHostContext _context;
    private readonly ScopeAggregator _scope;

    protected TestHost(IEnumerable<IDependencyContainer> containers, TestHostContext context)
    {
        _context = context;
        _scope = new ScopeAggregator(containers.ToArray(), context);
    }

    public void Execute<TService>(Action<TService> actionFn)
        where TService : class
    {
        actionFn(_scope.Resolve<TService>());
    }
    
    public void Execute<TService, TService2>(Action<TService, TService2> actionFn)
        where TService : class
        where TService2 : class
    {
        actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>());
    }

    public void Execute<TService, TService2, TService3>(Action<TService, TService2, TService3> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
    {
        actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>());
    }
    
    public void Execute<TService, TService2, TService3, TService4>(Action<TService, TService2, TService3, TService4> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
    {
        actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>(), _scope.Resolve<TService4>());
    }
    
    public TOutput Execute<TService, TOutput>(Func<TService, TOutput> actionFn)
        where TService : class
    {
        return actionFn(_scope.Resolve<TService>());
    }

    public TOutput Execute<TService, TService2, TOutput>(Func<TService, TService2, TOutput> actionFn)
        where TService : class
        where TService2 : class
    {
        return actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>());
    }

    public TOutput Execute<TService, TService2, TService3, TOutput>(Func<TService, TService2, TService3, TOutput> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
    {
        return actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>());
    }
    
    public TOutput Execute<TService, TService2, TService3, TService4, TOutput>(Func<TService, TService2, TService3, TService4, TOutput> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
    {
        return actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>(), _scope.Resolve<TService4>());
    }
    
    public async Task ExecuteAsync<TService>(Func<TService, Task> actionFn)
        where TService : class
    {
        await actionFn(_scope.Resolve<TService>());
    }

    public async Task ExecuteAsync<TService, TService2>(Func<TService, TService2, Task> actionFn)
        where TService : class
        where TService2 : class
    {
        await actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>());
    }

    public async Task ExecuteAsync<TService, TService2, TService3>(Func<TService, TService2, TService3, Task> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
    {
        await actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>());
    }
    
    public async Task ExecuteAsync<TService, TService2, TService3, TService4>(Func<TService, TService2, TService3, TService4, Task> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
    {
        await actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>(), _scope.Resolve<TService4>());
    }
    
    public async Task<TOutput> ExecuteAsync<TService, TOutput>(Func<TService, Task<TOutput>> actionFn)
        where TService : class
    {
        return await actionFn(_scope.Resolve<TService>());
    }

    public async Task<TOutput> ExecuteAsync<TService, TService2, TOutput>(Func<TService, TService2, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class
    {
        return await actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>());
    }

    public async Task<TOutput> ExecuteAsync<TService, TService2, TService3, TOutput>(Func<TService, TService2, TService3, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
    {
        return await actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>());
    }
    
    public async Task<TOutput> ExecuteAsync<TService, TService2, TService3, TService4, TOutput>(Func<TService, TService2, TService3, TService4, Task<TOutput>> actionFn)
        where TService : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
    {
        return await actionFn(_scope.Resolve<TService>(), _scope.Resolve<TService2>(), _scope.Resolve<TService3>(), _scope.Resolve<TService4>());
    }

    public void Dispose()
    {
        _scope.Dispose();
        _context.TestHostBagResolver.Remove(_context.TestHostId);
    }
}