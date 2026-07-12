---
title: "Captures: Capture vs Trapture"
description: How an artifact created in one phase threads into the next while the whole chain stays deferred.
---

Arrange and Act are **deferred**: `.Then(...)` only *records* a step; nothing runs until you `await`. That
raises a question — if the client isn't created until the chain runs, how does a later step refer to it? The
answer is a **capture**: a typed placeholder handed back immediately and filled when the step runs.

```csharp
var init = Capture.Start(out Capture<Client> client);   // client is empty for now
return arrange.Then(_ => init.Set(new Client(...)));      // filled when the chain runs
// after `await`: client.Value holds the created Client
```

`Start` hands you two things: the **capture** (via `out`, for the caller to read later) and an
**initializer** (returned, for the step to `Set`). This split is what makes deferral work — the caller gets a
reference to a value that doesn't exist yet.

## Two flavors

Mokkit ships two capture types with the same job but different ergonomics at the **use site**:

| | Read as | Use when |
| --- | --- | --- |
| **`Capture<T>`** | `capture.Value` (explicit) | You want the read to be visible — a deliberate `.Value`. |
| **`Trapture<T>`** | converts to `T` implicitly | The value flows transparently into later steps, and ceremony would just be noise. |

```csharp
Capture<Guid> a = ...;
DoSomething(a.Value);     // Capture — explicit

Trapture<Guid> b = ...;
DoSomething(b);           // Trapture — implicit conversion (TRansparent cAPTURE)
```

`Trapture` is the workhorse inside vocabulary, because ids and entities usually just need to flow from an
Arrange into the next Act or Inspect without anyone thinking about it:

```csharp
// NewClient hands back the id as a Trapture<Guid> ...
await Arrange.NewClient(out var clientId, WithName("Acme Corporation"));

// ... which the later Act and Inspect consume as a plain Guid, no .Value in sight.
await Act.UpdateClient(clientId, WithName("Acme Holdings"));
await Inspect.ApiClient(clientId, c => c.Name.ShouldBe("Acme Holdings"));
```

Reach for `Capture<T>` when you *want* the read to stand out — for instance a result whose `.Value` you unpack
and assert on deliberately.

## Producing a capture from a verb

An Arrange (or Act) verb that creates an artifact starts a capture, then sets it inside the deferred step:

```csharp
public static ITestArrange NewClient(
    this ITestArrange arrange, out Trapture<Guid> id, params ClientFieldFn[] fields)
{
    var capture = Trapture.Start(out id);              // hand the caller an empty capture
    return arrange.Then(async host =>
    {
        await host.ExecuteAsync<HttpClient>(async http =>
        {
            var result = await ClientApi.CreateAsync(http, Build(fields));
            capture.Set(result.ClientId!.Value);        // fill it when the chain runs
        });
    });
}
```

The interfaces behind this are small: `ICapture<out T>` exposes `Value`; `ICaptureInitializer<T>` exposes
`Set(T)`. A verb takes the *initializer* to write and hands back the *capture* to read.

## Deriving one value from another: `Ensure`

Often you don't want to capture the whole artifact, just something off it — an id — and you want it guarded as
non-empty before it threads onward. `Ensure` does derive-guard-capture in one step:

```csharp
await Inspect
    .WriteResult(result).Created()
    .Ensure(result, r => r.ClientId, out var clientId)   // non-empty Guid, captured
    .ApiClient(clientId, c => c.ShouldNotBeNull());
```

`Ensure` has its own [guide](/guides/ensure/); it's the idiomatic way to turn "the result's id" into a clean
capture the rest of the chain can use.

## Next

- **[Building your test vocabulary](/concepts/vocabulary/)** — where captures are produced and consumed.
- **[Scenario tests](/concepts/scenarios/)** — a captured id threading through a whole multi-step story.
