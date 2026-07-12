---
title: Write a custom container adapter
description: Not using Moq, NSubstitute, FakeItEasy, or Microsoft DI? The adapter contract is tiny — this walks through a real one end to end.
---

Mokkit ships adapters for Moq, NSubstitute, FakeItEasy, Microsoft DI, Autofac and Castle Windsor. If your stack
isn't in that list, you write an adapter — and the contract is small. This page walks the example's own
`SubstituteContainerBuilder` (a from-scratch NSubstitute container) to show the whole thing.

## The contract

A container is two interfaces:

```csharp
public interface IDependencyContainerBuilder
{
    Task PreInit();                                        // early setup
    Task Init();                                           // register your doubles/services
    Task PreBuild(IDependencyContainerBuilder[] builders); // coordinate with sibling containers
    TCollection? TryGetCollection<TCollection>() where TCollection : class;  // expose your registrations
    IDependencyContainer Build(ITestHostBagAccessor bagAccessor);
}

public interface IDependencyContainer            // Build() returns this
{
    IDependencyContainerScope BeginScope(TestHostContext context);   // one scope per stage
}
```

The four-phase lifecycle (**PreInit → Init → PreBuild → Build**) runs once for all builders at
`TestStageSetup.Create`. Most adapters only need `Init` (register) and `Build` (produce the container).

## The builder

Collect registrations in `Init`, hand them to the container in `Build`:

```csharp
public sealed class SubstituteContainerBuilder : IDependencyContainerBuilder
{
    private Func<ISubstituteCollection, Task>? _initFn;
    public SubstituteCollection SubstituteCollection { get; } = new();

    public SubstituteContainerBuilder UseInit(Func<ISubstituteCollection, Task> fn) { _initFn = fn; return this; }

    Task IDependencyContainerBuilder.PreInit() => Task.CompletedTask;
    Task IDependencyContainerBuilder.Init() => _initFn?.Invoke(SubstituteCollection) ?? Task.CompletedTask;
    Task IDependencyContainerBuilder.PreBuild(IDependencyContainerBuilder[] builders) => Task.CompletedTask;

    // Let a sibling container (e.g. DI) discover our registrations during its PreBuild.
    public TCollection? TryGetCollection<TCollection>() where TCollection : class =>
        SubstituteCollection as TCollection;

    IDependencyContainer IDependencyContainerBuilder.Build(ITestHostBagAccessor bagAccessor)
    {
        SubstituteCollection.MakeReadOnly();
        return new SubstituteContainer(SubstituteCollection, bagAccessor);
    }
}
```

## The container: one line is the whole contract

The container creates a scope per stage; the scope's job is to **publish each double into the test-host bag,
keyed by the service interface**. That single `bag.TryAdd` is what makes the mock→DI
[bridge](/concepts/containers/) work — `ResolveFromStage`, already in Mokkit, does the rest.

```csharp
public sealed class SubstituteContainer : BaseDependencyContainer, IDependencyContainer
{
    // ...ctor stores the collection + bag accessor...
    public IDependencyContainerScope BeginScope(TestHostContext context) =>
        new SubstituteScope(_collection, _bagAccessor);

    private sealed class SubstituteScope : IDependencyContainerScope
    {
        private readonly Dictionary<Type, object> _substitutes = new();

        public SubstituteScope(SubstituteCollection collection, ITestHostBagAccessor bagAccessor)
        {
            _bagAccessor = bagAccessor;
            foreach (var reg in collection.Registrations)
                _substitutes[reg.InnerType] = reg.Factory();   // fresh doubles per scope
        }

        public void OnAsyncScopeEnter()
        {
            var bag = _bagAccessor.Bag ?? throw new InvalidOperationException("bag is missing.");
            foreach (var (innerType, substitute) in _substitutes)
                bag.TryAdd(innerType, substitute);              // ← the whole contract
        }

        public T? TryResolve<T>() where T : class =>
            _substitutes.TryGetValue(typeof(T), out var s) ? (T)s : null;

        public void Dispose() { }
    }
}
```

That's it. Because an NSubstitute fake *is* the interface, the object in the bag, the dependency injected into
the real service, and the handle the test resolves are all the **same instance**. (A Moq adapter differs by one
line — it deposits `mock.Object` for injection while letting tests resolve the `Mock<T>` wrapper.)

## Shortcut: the mock-container base

If you're adapting a *mocking* library specifically, `BaseMockContainerBuilder<TMock>` in
`Mokkit.Containers.Common` already implements the four-phase lifecycle and the shared collection — the built-in
Moq/NSubstitute/FakeItEasy adapters are ~15 lines each on top of it. Start there unless you need full control.

## Next

- **[Containers & the mock→DI bridge](/concepts/containers/)** — the concept your adapter plugs into.
- **[Wire a real DI container](/guides/real-di-container/)** — the other half of the bridge.
