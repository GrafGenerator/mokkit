---
title: Advanced vocabulary techniques
description: Level up your Arrange/Inspect verbs with scopes, Ensure, and source-generated arranges — the tools that keep a growing vocabulary clean.
---

The [core idea](/concepts/vocabulary/) is simple: verbs are extension methods on `ITestArrange` / `ITestAct` /
`ITestInspect`. As a suite grows, a few Mokkit features become *vocabulary-authoring* techniques — ways to keep
those verbs terse and expressive rather than repetitive. Each has its own guide; this page is the map.

## Group assertions with scopes

When several checks concern one value, open a **[value scope](/guides/inspect-scopes/)** so the verbs read as
`.Thing(x).IsA().HasB()` — and a **context scope** when those checks must also run *inside* something (a single
`Assert.Multiple`, a transaction). Scopes turn a cluster of raw asserts into fluent, named steps.

```csharp
.WriteResult(result).Created()      // value scope over the result
.SaveResult(result).IsSuccess()     // context scope: runs inside Assert.Multiple
```

## Thread ids cleanly with `Ensure`

Deriving an id off an artifact and guarding it non-empty is a per-test ritual. **[`Ensure`](/guides/ensure/)**
collapses derive-guard-capture into one step, so ids flow between phases without `!.Value` and null-checks:

```csharp
.Ensure(result, r => r.ClientId, out var clientId)   // non-empty Guid, captured
```

## Generate the boring arranges

A verb whose whole job is "build this DTO from these fields and capture it" is pure boilerplate. Let
**[`[MokkitCapture]`](/guides/mokkit-capture/)** write the body — you declare a partial method, the generator
constructs the object (by constructor or object-initializer) and captures it:

```csharp
[MokkitCapture]
public static partial ITestArrange StatusChanged(
    this ITestArrange arrange, out Trapture<StatusChangedMessage> message,
    Guid clientId, string? name, string? email, string? phone, int status);
```

## The pattern behind all three

Each of these keeps *intent* in the test body and *mechanism* in the vocabulary. A scope hides how assertions
are grouped; `Ensure` hides the guard; `[MokkitCapture]` hides the constructor call. The test keeps reading like
a sentence while the plumbing lives — and is reused — in the verb. That's the whole game: as the vocabulary
compounds, tests get *shorter*, not longer.

## Next

- **[Value & context scopes](/guides/inspect-scopes/)** · **[Ensure](/guides/ensure/)** · **[`[MokkitCapture]`](/guides/mokkit-capture/)**
- **[Write a custom container adapter](/guides/custom-container-adapter/)** — extend Mokkit itself.
