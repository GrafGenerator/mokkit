using System;

namespace Mokkit.Containers.Bag;

/// <summary>
/// A single bag registration — either a shared singleton instance or a per-scope factory.
/// </summary>
internal sealed class BagRegistration
{
    private BagRegistration(bool isSingleton, object? instance, Func<object>? factory)
    {
        IsSingleton = isSingleton;
        Instance = instance;
        Factory = factory;
    }

    /// <summary>Gets a value indicating whether this registration is a shared singleton instance.</summary>
    public bool IsSingleton { get; }

    /// <summary>Gets the singleton instance, or <c>null</c> for factory registrations.</summary>
    public object? Instance { get; }

    /// <summary>Gets the per-scope factory, or <c>null</c> for singleton registrations.</summary>
    public Func<object>? Factory { get; }

    /// <summary>Creates a shared singleton registration.</summary>
    public static BagRegistration Singleton(object instance) => new(true, instance, null);

    /// <summary>Creates a per-scope factory registration.</summary>
    public static BagRegistration Scoped(Func<object> factory) => new(false, null, factory);
}
