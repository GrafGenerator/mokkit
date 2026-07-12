---
title: "Ensure: derive, guard, capture"
description: Turn "the result's id" into a clean, non-empty capture in one step — so ids flow between phases without null-checks littering the test.
---

A recurring need: take a value off an artifact (an entity's id), make sure it isn't empty, and thread it into
later steps. Done by hand that's `result.ClientId!.Value` plus a guard plus a capture — noise that repeats in
every test. **`Ensure`** does all three in one call: **derive → guard-as-non-empty → capture**.

## In Inspect (eager)

The inspect overloads run immediately, so the capture is filled the moment you call them — perfect for pulling
an id off a result you just asserted, then observing by it:

```csharp
await Inspect
    .WriteResult(result).Created()
    .Ensure(result, r => r.ClientId, out var clientId)   // non-empty Guid, captured now
    .ApiClient(clientId, c => c.Name.ShouldBe("Acme Corporation"))
    .EventPublished("clients.created", clientId);
```

`Ensure` is type-aware about "empty": it rejects `Guid.Empty`, `null`, `""`, `0`, and empty collections, throwing
a clear failure instead of letting a bogus value flow onward. There's also a direct form when you already hold
the value — `.Ensure(someValue, out var captured)`.

## In Arrange (deferred)

The arrange overloads are **deferred** — they capture a `Trapture<T>` that's filled when the chain runs, so you
can derive from a value another arrange step produces:

```csharp
public static ITestArrange Ensure<TSource, T>(
    this ITestArrange arrange, ICapture<TSource> source, Func<TSource, T> selector,
    out Trapture<T> captured, string? because = null) where TSource : class
{
    var initializer = Trapture.Start(out captured);
    return arrange.Then(_ =>
    {
        var value = source.Value ?? throw new InvalidOperationException("Ensure: source capture is not initialized.");
        initializer.Set(EnsureGuard.NotEmpty(selector(value), because));
    });
}
```

Use it to guard a derived id before later arranges consume it. There's also a thunk form —
`.Ensure(() => client.Value!.Id, out var id)` — for values built from more than one capture. Both hand back a
[`Trapture<T>`](/concepts/captures/), so the id flows transparently.

## Why it's worth a verb

Without `Ensure`, ids arrive as `result.ClientId!.Value` — a null-forgiving operator and an unchecked
assumption in every test. `Ensure` replaces that with a single, self-documenting step that fails loudly and
early if the precondition ("there *is* an id") doesn't hold. It's the idiomatic bridge between an artifact and
the [captures](/concepts/captures/) that carry it forward.

## Next

- **[Captures: Capture vs Trapture](/concepts/captures/)** — what `Ensure` produces.
- **[Building your test vocabulary](/concepts/vocabulary/)** — `Ensure` as a vocabulary-authoring technique.
