namespace Mokkit.Example1.Unit.Tests.Containers;

/// <summary>
/// One registered substitute: the service interface it stands in for (<see cref="InnerType"/>)
/// and a factory that produces a fresh NSubstitute fake for it.
/// </summary>
public sealed class SubstituteRegistration
{
    public SubstituteRegistration(Type innerType, Func<object> factory)
    {
        InnerType = innerType;
        Factory = factory;
    }

    /// <summary>The service interface this substitute is registered for (e.g. <c>IDistributedCache</c>).</summary>
    public Type InnerType { get; }

    /// <summary>Creates a fresh substitute instance. Called once per test scope.</summary>
    public Func<object> Factory { get; }
}
