using System;

namespace Mokkit.Containers.Bag;

/// <summary>
/// The registration surface of a <see cref="BagContainerBuilder"/>, handed to <see cref="BagContainerBuilder.UseInit"/>
/// callbacks so services can be registered without exposing the builder lifecycle.
/// </summary>
public interface IBagRegistrar
{
    /// <summary>
    /// Registers a pre-built instance to be resolved by its own type <typeparamref name="T"/>.
    /// The instance is shared across all scopes and is <b>not</b> disposed by the container — the caller owns its lifetime.
    /// </summary>
    /// <typeparam name="T">The type under which the instance is resolved (may be an interface or a concrete type).</typeparam>
    /// <param name="instance">The instance to store.</param>
    /// <returns>The registrar for fluent chaining.</returns>
    IBagRegistrar AddInstance<T>(T instance) where T : class;

    /// <summary>
    /// Registers a factory that produces a value resolved by its type <typeparamref name="T"/>.
    /// The factory is invoked at most once per scope; the produced value is disposed on scope disposal if it is <see cref="IDisposable"/>.
    /// </summary>
    /// <typeparam name="T">The type under which the produced value is resolved (may be an interface or a concrete type).</typeparam>
    /// <param name="factory">The factory that creates the value.</param>
    /// <returns>The registrar for fluent chaining.</returns>
    IBagRegistrar AddFactory<T>(Func<T> factory) where T : class;
}
