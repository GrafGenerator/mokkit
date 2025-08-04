using System;
using Microsoft.Extensions.DependencyInjection;
using Mokkit.Suite;

namespace Mokkit.Containers.Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents a dependency injection container that wraps Microsoft.Extensions.DependencyInjection's IServiceProvider.
/// This container integrates with the Mokkit testing framework to provide scoped service resolution using the Microsoft DI container.
/// </summary>
public class ServiceProviderContainer : BaseDependencyContainer, IDependencyContainer
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceProviderContainer"/> class with the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider to wrap for dependency injection.</param>
    public ServiceProviderContainer(IServiceProvider serviceProvider)
    {
        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    /// <summary>
    /// Creates a new dependency injection scope within the specified test host context.
    /// The scope provides isolated service resolution using Microsoft.Extensions.DependencyInjection scoping.
    /// </summary>
    /// <param name="context">The test host context that defines the scope parameters.</param>
    /// <returns>A new <see cref="IDependencyContainerScope"/> for scoped service resolution.</returns>
    public IDependencyContainerScope BeginScope(TestHostContext context)
    {
        return new DependencyScope(_scopeFactory, context);
    }

    /// <summary>
    /// Represents a scoped dependency injection container that provides isolated service resolution within a specific test context.
    /// This class wraps Microsoft.Extensions.DependencyInjection's IServiceScope to provide Mokkit-compatible scoping.
    /// </summary>
    private class DependencyScope : IDependencyContainerScope
    {
        private readonly TestHostContext _context;
        private readonly IServiceScope _serviceScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyScope"/> class with the specified scope factory and context.
        /// </summary>
        /// <param name="scopeFactory">The service scope factory to create the underlying service scope.</param>
        /// <param name="context">The test host context that defines the scope parameters.</param>
        public DependencyScope(IServiceScopeFactory scopeFactory, TestHostContext context)
        {
            _context = context;
            _serviceScope = scopeFactory.CreateScope();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// This method disposes the underlying service scope.
        /// </summary>
        public void Dispose()
        {
            _serviceScope.Dispose();
        }

        /// <summary>
        /// Notifies the scope that an asynchronous operation is entering the scope.
        /// This implementation does not require any special handling for async scope entry.
        /// </summary>
        public void OnAsyncScopeEnter()
        {
        }

        /// <summary>
        /// Attempts to resolve a service of the specified type from the Microsoft.Extensions.DependencyInjection container.
        /// </summary>
        /// <typeparam name="T">The type of service to resolve. Must be a reference type.</typeparam>
        /// <returns>An instance of the requested service type, or <c>null</c> if the service is not registered.</returns>
        public T? TryResolve<T>() where T : class
        {
            return _serviceScope.ServiceProvider.GetService<T>();
        }
    }
}