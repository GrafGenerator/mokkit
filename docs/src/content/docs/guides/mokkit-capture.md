---
title: "Source-generated arranges with [MokkitCapture]"
description: Let the generator write the boilerplate body of a "build this object and capture it" arrange verb, so your vocabulary file is just declarations.
---

Many arrange verbs are pure boilerplate: start a capture, build an object from the parameters, set the capture
when the chain runs. `[MokkitCapture]` writes that body for you. You declare a **partial** arrange method; the
source generator (bundled in the `Mokkit` package) fills it in.

## Declare, don't implement

Mark a partial extension method with `[MokkitCapture]`. Its parameters describe the object to build; the `out`
capture receives it:

```csharp
public static partial class ArrangeMessages
{
    [MokkitCapture]
    public static partial ITestArrange StatusChanged(
        this ITestArrange arrange,
        out Trapture<StatusChangedMessage> message,
        Guid clientId, string? name, string? email, string? phone, int status);
}
```

That's the whole file — no body. The generator produces one equivalent to a hand-written
`Trapture.Start(out message)` + `arrange.Then(_ => set.Set(new StatusChangedMessage { ... }))`. Call it like any
verb:

```csharp
await Arrange
    .StatusChanged(out var message, clientId, name, email, phone, (int)ClientStatus.Suspended);
```

## Two construction strategies

The generator inspects the captured type and builds it the right way:

```csharp
public sealed record Foo(int Value, string Name);   // positional → constructor
public sealed record Baz { public int Code { get; init; } public string Label { get; init; } = ""; }  // init props → object initializer

[MokkitCapture]
public static partial ITestArrange ArrangeFoo(this ITestArrange a, out Trapture<Foo> foo, int value, string name);
// generated: new Foo(value, name)

[MokkitCapture]
public static partial ITestArrange ArrangeBaz(this ITestArrange a, out Trapture<Baz> baz, int code, string label);
// generated: new Baz { Code = code, Label = label }
```

- **Positional records / constructor types** → the generator calls the **constructor**, matching parameters by
  name.
- **Types with `init` properties** → it uses an **object initializer**.

Either `Capture<T>` or `Trapture<T>` works as the out parameter — pick per how the value flows (see
[Capture vs Trapture](/concepts/captures/)). Unmentioned fields are simply left at their defaults — handy for
testing "what happens when the contact fields are null":

```csharp
[MokkitCapture]
public static partial ITestArrange IncomingStatusChangeMissingContact(
    this ITestArrange arrange, out Capture<ClientStatusChangedMessage> message, Guid clientId, int status);
// builds the message from ClientId + Status; leaves Name/Email/Phone null
```

## When to use it

Reach for `[MokkitCapture]` when an arrange verb is *only* "construct this DTO/command/message from these
fields and capture it". When the verb needs real behaviour — call an API, seed a database, set up a mock —
write it by hand with `.Then(...)`. The two mix freely in the same vocabulary file.

:::note[It's in the box]
The generator ships inside the `Mokkit` package as an analyzer — no extra reference. The declaring class and
method must be `partial`.
:::

## Next

- **[Captures: Capture vs Trapture](/concepts/captures/)** — what the generated body produces.
- **[Building your test vocabulary](/concepts/vocabulary/)** — where generated arranges fit.
