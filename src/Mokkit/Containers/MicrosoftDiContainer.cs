using System;
using Microsoft.Extensions.DependencyInjection;

namespace Mokkit.Containers;

public class MicrosoftDiContainer : BaseDependencyContainer, IDependencyContainer
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MicrosoftDiContainer(IServiceProvider serviceProvider)
    {
        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    public IDependencyContainerScope BeginScope()
    {
        return new DependencyScope(_scopeFactory);
    }

    private class DependencyScope : IDependencyContainerScope
    {
        private readonly IServiceScope _serviceScope;

        public DependencyScope(IServiceScopeFactory scopeFactory)
        {
            _serviceScope = scopeFactory.CreateScope();
        }
        
        public void Dispose()
        {
            _serviceScope.Dispose();
        }

        public T? TryResolve<T>()
        {
            return _serviceScope.ServiceProvider.GetService<T>();
        }
    }
}