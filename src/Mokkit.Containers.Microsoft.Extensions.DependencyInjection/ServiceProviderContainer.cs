using System;
using Microsoft.Extensions.DependencyInjection;
using Mokkit.Suite;

namespace Mokkit.Containers.Microsoft.Extensions.DependencyInjection;

public class ServiceProviderContainer : BaseDependencyContainer, IDependencyContainer
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ServiceProviderContainer(IServiceProvider serviceProvider)
    {
        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    public IDependencyContainerScope BeginScope(TestHostContext context)
    {
        return new DependencyScope(_scopeFactory, context);
    }

    private class DependencyScope : IDependencyContainerScope
    {
        private readonly IServiceScope _serviceScope;

        public DependencyScope(IServiceScopeFactory scopeFactory, TestHostContext context)
        {
            _serviceScope = scopeFactory.CreateScope();

            var stageResolveSetup = _serviceScope.ServiceProvider.GetRequiredService<IStageResolveSetup>();
            
            stageResolveSetup.SetBag(context.TestHostBagResolver.Get(context.TestHostId));
        }
        
        public void Dispose()
        {
            _serviceScope.Dispose();
        }

        public T? TryResolve<T>() where T : class
        {
            return _serviceScope.ServiceProvider.GetService<T>();
        }
    }
}