---
title: Arrange / Act / Inspect
description: The three-phase shape every Mokkit test follows, and the discipline that keeps tests honest.
---

Every Mokkit test tells the same three-part story. It's the classic Arrange-Act-Assert, with the assert
phase renamed **Inspect** to make its one rule explicit: it only *observes*.

## Arrange

`stage.Arrange()` starts a fluent chain. Each `.Then(...)` registers a step; **`await`-ing the chain runs
the steps in order.** A step receives the test host, so it can resolve and configure services:

```csharp
await stage.Arrange()
    .Then(host => host.Execute<Mock<IClock>>(clock =>
        clock.Setup(x => x.UtcNow).Returns(FixedNow)))
    .Then(async host => await host.ExecuteAsync<Db>(db => db.Seed(...)));
```

Arrange is **deferred**: `.Then(...)` only *records* the step; nothing happens until you `await`. That's what
lets an arrange hand back a **capture** — a placeholder for a value that doesn't exist yet:

```csharp
var init = Capture.Start(out Capture<Client> client);      // client is empty for now
return arrange.Then(_ => init.Set(new Client(...)));         // filled when the chain runs
```

After the `await`, `client.Value` holds the created client. Captures are how the artifacts an arrange
creates flow into later steps. See [Capture vs Trapture](/concepts/captures/).

## Act

Act is the one thing under test — and, like Arrange and Inspect, it's a **first-class phase**. `stage.Act()`
starts a fluent chain whose `.Then(...)` steps resolve services from the stage and run them. Because Act is
symmetric with Arrange, an Act operation is reusable **vocabulary** too, and — like Arrange — it may *produce*
an artifact the later Inspect observes.

An Act comes in three flavors, depending on what (if anything) the operation hands back:

```csharp
// Void — the operation's effects are observed downstream (e.g. a message is emitted onto a bus).
await stage.Act().Then(host => host.ExecuteAsync<IProducer>(p => p.ProduceAsync(topic, message)));

// Return — the operation returns its artifact directly (the familiar `var result = await ...`).
var result = await stage.Act().Returning(host =>
    host.ExecuteAsync<SaveClientHandler, SaveResult>(handler => handler.Handle(command)));

// Capture — the operation threads its result forward through an out capture, exactly like Arrange.
await stage.Act().SaveClient(out var result, command);
```

`Execute`/`ExecuteAsync` come in 1-to-4-service arities, so an Act can pull several collaborators at once.
As with Arrange and Inspect, teams give their Acts domain names — `Act.CreateClient(...)`,
`Act.ProduceStatusChanged(...)` — so the test body reads as the story it tells. When a test is a whole
*sequence* of Act steps interleaved with checks, see **[Scenario tests](/concepts/scenarios/)**.

## Inspect

`stage.Inspect()` starts another fluent chain, awaited the same way. Inspect steps **read** the world —
query the database, verify a mock, poll an endpoint — and assert on it. They must not mutate state:

```csharp
await stage.Inspect()
    .SaveResult(result).IsSuccess()          // a value scope over the result
    .DbClientExists(id)                       // reads the database
    .EventPublished("clients.created", id);   // verifies a mock
```

Inspect also offers two power tools, covered in the guides:

- **Value scopes** — `ThenValueScope(value)` opens a focused block of assertions over one value.
- **Parallel inspects** — `ThenAll(...)` runs independent observations concurrently while the chain stays
  readable.

## The discipline: produce vs observe

One rule keeps tests trustworthy and reads across the whole suite:

> **Arrange and Act *produce* artifacts. Inspect only *observes* them.**

If setting something up needs a side effect, it belongs in Arrange (or is the Act itself). Inspect never
creates or changes state — so you can always trust that a failing Inspect is reporting on what Act did, not
on something Inspect itself caused.

The verbs in these three chains — `SaveResult`, `DbClientExists`, `EventPublished` — are the heart of the
matter. They're your project's **[test vocabulary](/concepts/vocabulary/)**.
