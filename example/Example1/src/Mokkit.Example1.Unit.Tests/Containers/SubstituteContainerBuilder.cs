using Mokkit.Containers;
using Mokkit.Suite;

namespace Mokkit.Example1.Unit.Tests.Containers;

/// <summary>
/// A Mokkit dependency-container builder backed by NSubstitute. It is the NSubstitute counterpart
/// of <c>MoqContainerBuilder</c> and shows that Mokkit is not tied to any particular mocking library:
/// implement <see cref="IDependencyContainerBuilder"/>, put fakes into the test-host bag keyed by the
/// service interface, and the existing <c>ResolveFromStage</c> glue injects them into real services.
/// </summary>
public sealed class SubstituteContainerBuilder : IDependencyContainerBuilder
{
    private Func<ISubstituteCollection, Task>? _initFn;

    public SubstituteCollection SubstituteCollection { get; } = new();

    /// <summary>Registers the substitutes for this container.</summary>
    public SubstituteContainerBuilder UseInit(Func<ISubstituteCollection, Task> fn)
    {
        _initFn = fn;
        return this;
    }

    Task IDependencyContainerBuilder.PreInit() => Task.CompletedTask;

    Task IDependencyContainerBuilder.Init() =>
        _initFn?.Invoke(SubstituteCollection) ?? Task.CompletedTask;

    Task IDependencyContainerBuilder.PreBuild(IDependencyContainerBuilder[] builders) => Task.CompletedTask;

    public TCollection? TryGetCollection<TCollection>() where TCollection : class =>
        SubstituteCollection as TCollection;

    IDependencyContainer IDependencyContainerBuilder.Build(ITestHostBagAccessor bagAccessor)
    {
        SubstituteCollection.MakeReadOnly();
        return new SubstituteContainer(SubstituteCollection, bagAccessor);
    }
}
