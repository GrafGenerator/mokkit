---
title: Parallel inspects with ThenAll
description: Run independent observations concurrently — three downstream effects at once — while the chain stays ordered and readable.
---

After an act, you often need to check several *independent* effects: the API read, the database row, the
published event. Checked one after another they serialise — and each may involve a
[poll with a timeout](/guides/eventually-consistent/), so the waits add up. **`ThenAll`** runs a group of
inspects **concurrently**, then the chain continues in order.

## The branch-builder form

The most readable form takes branch builders — each branch is its own little inspect sub-chain:

```csharp
await Inspect
    .WriteResult(result).Created()
    .Ensure(result, r => r.ClientId, out var clientId)
    .ThenAll(
        b => b.ApiClient(clientId, c => c.Name.ShouldBe("Acme Corporation")),
        b => b.DbClient(clientId, c => c.ShouldNotBeNull()),
        b => b.EventPublished("clients.created", clientId));
```

The three observations run at once; the slowest one determines how long the group takes, not the sum. A branch
can chain multiple steps — `b => b.Then(...).Then(...)` — and they run within that branch in order.

## Ordering guarantees

`ThenAll` is a concurrent *group inside* an otherwise ordered chain. Steps before it complete first; steps after
it run only once every branch has finished:

```csharp
await Inspect
    .Then(_ => Add(1))
    .Then(_ => Add(2))
    .ThenAll(b => b.Then(_ => Add(3)), b => b.Then(_ => Add(4)))   // 3 and 4 in either order
    .Then(_ => Add(5));                                             // strictly after both
// order: 1, 2, {3,4 in some order}, 5
```

Concurrent resolves from the stage are safe — the container hands every branch the same cached instance — so
even a high fan-out (dozens of branches all resolving the same service) is race-free.

## Forms

`ThenAll` also accepts plain inspect functions when you don't need sub-chains:

```csharp
ITestInspect ThenAll(params Func<ITestInspect, ITestInspect>[] branches);  // branch builders (shown above)
ITestInspect ThenAll(params InspectAsyncFn[] inspectFns);                   // async funcs
ITestInspect ThenAll(params InspectFn[] inspectFns);                        // sync funcs
```

:::tip[Use it for independent effects]
`ThenAll` is for observations that don't depend on each other. If one check must happen before another — read a
value, then assert on it — keep those in sequence; only parallelise what's genuinely independent.
:::

## Next

- **[Async / eventually-consistent assertions](/guides/eventually-consistent/)** — why parallelising the waits matters.
- **[Full black-box E2E](/guides/end-to-end/)** — where several downstream effects are checked at once.
