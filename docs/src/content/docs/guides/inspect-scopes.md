---
title: Value & context scopes in Inspect
description: Focus a run of assertions on one value, and wrap them in a context — a single Assert.Multiple, a transaction — with inspect scopes.
---

Most inspect verbs read the world and assert. Sometimes, though, several assertions all concern **one value** —
a result object, a fetched entity — and you want them grouped. That's an **inspect scope**: a focused block of
checks over a single value, opened with `ThenValueScope`.

## A value scope

`ThenValueScope(value)` returns an `ITestInspectScope<T>`, and verbs on that scope receive the value directly.
The write-result vocabulary is a clean example — `WriteResult` opens the scope, `Created`/`Updated`/`Rejected`
assert within it:

```csharp
public static ITestInspectScope<ClientWriteResult> WriteResult(this ITestInspect inspect, ClientWriteResult result) =>
    inspect.ThenValueScope(result);

public static ITestInspectScope<ClientWriteResult> Created(this ITestInspectScope<ClientWriteResult> inspect) =>
    inspect.Then((result, _) =>
    {
        result.Status.ShouldBe(HttpStatusCode.Created);
        result.ClientId.ShouldNotBeNull();
    });
```

Reads naturally at the call site, and flows back into the normal chain afterwards:

```csharp
await Inspect
    .WriteResult(result).Created()          // scope over the result
    .ApiClient(clientId, c => ...);          // back to ordinary inspects
```

## A context scope

A scope can also wrap *how* its assertions run by passing a context callback to `ThenValueScope`. The
integration suite uses this to run every check inside a single NUnit `Assert.Multiple`, so one failing field
doesn't hide the others:

```csharp
public static ITestInspectScope<SaveClientCommandResult> SaveResult(
    this ITestInspect inspect, SaveClientCommandResult result) =>
    inspect.ThenValueScope(result, async (_, execute) =>
    {
        await Assert.MultipleAsync(async () => await execute());   // wrap the scope's steps
    });

public static ITestInspectScope<SaveClientCommandResult> IsSuccess(
    this ITestInspectScope<SaveClientCommandResult> inspect, Guid? expectedClientId = null) =>
    inspect.Then((result, _) =>
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Exception, Is.Null);
        if (expectedClientId.HasValue) Assert.That(result.ClientId, Is.EqualTo(expectedClientId.Value));
    });
```

The `execute` delegate runs the scope's chained steps; wrapping it lets you impose a context — `Assert.Multiple`
here, but equally a transaction, a timing block, or a retry. The value and the context travel together, so every
verb on the scope gets both.

## When to use which

- **Plain inspect verb** — an observation that stands alone (`ApiClient`, `EventPublished`).
- **Value scope** — several assertions about the *same* value read cleanly as `.Thing(x).IsA().HasB()`.
- **Context scope** — those grouped assertions also need to run *inside* something (multi-assert, transaction).

## Next

- **[Building your test vocabulary](/concepts/vocabulary/)** — scopes are a verb-authoring technique.
- **[Snapshot assertions with Verify](/guides/verify-snapshots/)** — often used right after a value scope.
- **[Parallel inspects with ThenAll](/guides/thenall/)** — the other Inspect power tool.
