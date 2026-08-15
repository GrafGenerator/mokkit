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

The selector form comes in three shapes, picked by what you project:

| Source | Selector | Use for |
| --- | --- | --- |
| a plain object | `r => r.ClientId` (`Guid?`) | unwrapping a nullable **struct** member |
| a plain object | `r => r.Name` (`string?`) | guarding a nullable **reference** member |
| a [capture](/concepts/captures/) | `c => c.Id` (anything) | projecting off a capture Arrange already filled |

The capture-shaped form is the one to reach for when the same id is read several times in one chain — guard it
once, then hand the plain value to every step that follows:

```csharp
await Inspect
    .Ensure(seeded, c => c.Id, out var clientId)     // one guarded read ...
    .GetResult(result).Found(clientId)               // ... reused from here on
    .CacheUpdated(clientId);
```

It reports an uninitialized capture as its own failure, rather than as a merely "empty" value. For a **single**
read, [`Prop`](/concepts/captures/) is lighter: `.CacheQueried(client.Prop(c => c.Id))`.

## On the capture itself

`Ensure` is a *chain* verb — it guards a value and threads it onward. The same guard is available directly on
any [capture](/concepts/captures/), for when you just need to read it:

| | Guards | Hands back |
| --- | --- | --- |
| `capture.EnsureValue` | the captured value, non-empty | the whole value |
| `capture.Prop(c => c.Id)` | the capture | the projected member, as-is |
| `.Ensure(capture, c => c.Id, out var id)` | both, inside the chain | a reusable, guarded local |

All three share one definition of "empty", so `client.EnsureValue` rejects exactly what
`.Ensure(client.Value, out _)` would.

## In Arrange (deferred)

The arrange overloads are **deferred** — they capture a `Trapture<T>` that's filled when the chain runs, so you
can derive from a value another arrange step produces:

```csharp
public static ITestArrange Ensure<TSource, T>(
    this ITestArrange arrange, ICapture<TSource> source, Func<TSource, T> selector,
    out Trapture<T> captured, string? because = null)
{
    var initializer = Trapture.Start(out captured);
    return arrange.Then(_ =>
    {
        if (source.Value is not { } value) throw new InvalidOperationException("Ensure: uninitialized capture.");
        initializer.Set(EnsureGuard.NotEmpty(selector(value), because));
    });
}
```

Use it to guard a derived id before later arranges consume it. There's also a thunk form —
`.Ensure(() => client.Prop(c => c.Id) + suffix, out var key)` — for values built from more than one capture.
Both hand back a [`Trapture<T>`](/concepts/captures/), so the id flows transparently. The source capture can
hold anything, a value type included: `.Ensure(clientId, g => g.ToString(), out var key)`.

## Why it's worth a verb

Without `Ensure`, ids arrive as `result.ClientId!.Value` — a null-forgiving operator and an unchecked
assumption in every test. `Ensure` replaces that with a single, self-documenting step that fails loudly and
early if the precondition ("there *is* an id") doesn't hold. It's the idiomatic bridge between an artifact and
the [captures](/concepts/captures/) that carry it forward — `Ensure` being the guarded *derivation*, and
[`Prop`](/concepts/captures/) the guarded *read*.

## Next

- **[Captures: Capture vs Trapture](/concepts/captures/)** — what `Ensure` produces.
- **[Building your test vocabulary](/concepts/vocabulary/)** — `Ensure` as a vocabulary-authoring technique.
