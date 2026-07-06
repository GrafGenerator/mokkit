using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mokkit.Containers;
using Mokkit.Suite;

namespace Mokkit.Containers.Bag;

/// <summary>
/// Builds a <see cref="BagContainer"/> — a minimal type-to-object container with no external DI dependency and no
/// auto-wiring. Register pre-built instances with <see cref="AddInstance{T}"/> or per-scope values with
/// <see cref="AddFactory{T}"/>, either directly on the builder or from a <see cref="UseInit"/> callback.
/// </summary>
public class BagContainerBuilder : IDependencyContainerBuilder, IBagRegistrar
{
    private readonly Dictionary<Type, BagRegistration> _registrations = new();
    private Func<IBagRegistrar, Task>? _initFn;

    /// <summary>
    /// Registers a pre-built instance to be resolved by its own type <typeparamref name="T"/>.
    /// The instance is shared across all scopes and is <b>not</b> disposed by the container.
    /// </summary>
    /// <typeparam name="T">The type under which the instance is resolved.</typeparam>
    /// <param name="instance">The instance to store.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public BagContainerBuilder AddInstance<T>(T instance) where T : class
    {
        _registrations[typeof(T)] = BagRegistration.Singleton(instance);
        return this;
    }

    /// <summary>
    /// Registers a factory producing a value resolved by its type <typeparamref name="T"/>.
    /// The factory is invoked at most once per scope; the value is disposed on scope disposal if it is <see cref="IDisposable"/>.
    /// </summary>
    /// <typeparam name="T">The type under which the produced value is resolved.</typeparam>
    /// <param name="factory">The factory that creates the value.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public BagContainerBuilder AddFactory<T>(Func<T> factory) where T : class
    {
        _registrations[typeof(T)] = BagRegistration.Scoped(() => factory());
        return this;
    }

    /// <summary>
    /// Configures a callback that registers services during the initialization phase, mirroring the
    /// <c>UseInit</c> style of the other Mokkit container builders.
    /// </summary>
    /// <param name="fn">The registration callback, receiving the bag registrar.</param>
    /// <returns>The builder for fluent chaining.</returns>
    public BagContainerBuilder UseInit(Func<IBagRegistrar, Task> fn)
    {
        _initFn = fn;
        return this;
    }

    IBagRegistrar IBagRegistrar.AddInstance<T>(T instance)
    {
        AddInstance(instance);
        return this;
    }

    IBagRegistrar IBagRegistrar.AddFactory<T>(Func<T> factory)
    {
        AddFactory(factory);
        return this;
    }

    Task IDependencyContainerBuilder.PreInit() => Task.CompletedTask;

    Task IDependencyContainerBuilder.Init() => _initFn?.Invoke(this) ?? Task.CompletedTask;

    Task IDependencyContainerBuilder.PreBuild(IDependencyContainerBuilder[] builders) => Task.CompletedTask;

    /// <summary>
    /// Attempts to retrieve this builder's registration surface for cross-builder coordination.
    /// </summary>
    /// <typeparam name="TCollection">The requested collection type.</typeparam>
    /// <returns>This builder cast to <typeparamref name="TCollection"/>, or <c>null</c> if the cast is not possible.</returns>
    public TCollection? TryGetCollection<TCollection>() where TCollection : class => this as TCollection;

    IDependencyContainer IDependencyContainerBuilder.Build(ITestHostBagAccessor bagAccessor) =>
        new BagContainer(_registrations);
}
