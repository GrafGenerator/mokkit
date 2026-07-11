---
title: Scenario tests
description: Tell a whole story as a sequence of Arrange / Act / Inspect blocks — one test, many steps, each verified in turn.
---

A single Arrange / Act / Inspect triple describes one moment: set up, do the thing, check the result. But
some behaviour is a **story** — a client is created, then renamed, then suspended — and you want to confirm
the system is correct *after each step*, not just at the end. Because [Act is a first-class
phase](/concepts/aai/#act), you don't need a new construct for that: a scenario is just a **sequence of AAI
blocks**, written as plain awaited statements.

```csharp
// Born — create the client, and confirm it starts out Active.
await Arrange
    .NewClient(out var clientId, WithName("Acme Corporation"), WithStatus(ClientStatus.Active));

await Inspect
    .ApiClient(clientId, c => c.Status.ShouldBe((int)ClientStatus.Active));

// Renamed — update it through the API. This act returns its artifact, which the next Inspect asserts.
var renamed = await Act
    .UpdateClient(clientId, WithName("Acme Holdings"));

await Inspect
    .WriteResult(renamed).Updated()
    .ApiClient(clientId, c => c.Name.ShouldBe("Acme Holdings"));

// Suspended — an upstream message carries the change. This act is void; its effects surface downstream.
await Arrange
    .ArrangeStatusChanged(out var suspend, clientId, "Acme Holdings", Email, Phone, (int)ClientStatus.Suspended);

await Act
    .ProduceStatusChanged(clientId, suspend);

await Inspect
    .ApiClientEventually(clientId, c => c.Status == (int)ClientStatus.Suspended)
    .DbClient(clientId, c => c!.Status.ShouldBe(ClientStatus.Suspended))
    .EventPublished("clients.updated", clientId);
```

Read top to bottom, that test *is* its own specification: build → check → act → check → act → check. No
Gherkin, no step-binding file — just compilable C# that happens to read like the scenario it verifies.

## How it holds together

**The captured id threads through the whole story.** `NewClient` hands back `clientId` as a
[`Trapture<Guid>`](/concepts/captures/), which converts transparently wherever a `Guid` is expected — so every
later Act and Inspect refers to the same client without any plumbing.

**`Arrange`, `Act` and `Inspect` are properties on the fixture**, each starting a fresh chain:

```csharp
protected ITestArrange Arrange => Stage.Arrange();
protected ITestAct     Act     => Stage.Act();
protected ITestInspect Inspect => Stage.Inspect();
```

**Every step is real vocabulary**, so the story stays in your domain's language. Act verbs live right next to
the Arrange and Inspect verbs for the same feature:

```csharp
public static class ActClientApi
{
    // Return flavor — the write returns its artifact, asserted by the very next Inspect.
    public static ITestAct<ClientWriteResult> UpdateClient(
        this ITestAct act, Guid clientId, params ClientFieldFn[] fields) =>
        act.Returning(host => host.ExecuteAsync<HttpClient, ClientWriteResult>(
            http => ClientApi.UpdateAsync(http, clientId, Build(fields))));

    // Void flavor — fire the message; its effects are observed downstream in Inspect.
    public static ITestAct ProduceStatusChanged(this ITestAct act, Guid clientId, StatusChangedMessage message) =>
        act.Then(host => host.ExecuteAsync<IProducer<string, string>>(async producer =>
        {
            await producer.ProduceAsync("clients.status-changed", Serialize(clientId, message));
            producer.Flush(TimeSpan.FromSeconds(5));
        }));
}
```

See [Building your test vocabulary](/concepts/vocabulary/) for how Arrange, Act and Inspect verbs are authored.

## When to reach for a scenario

Scenarios shine for **end-to-end and integration** tests, where a feature is a lifecycle and each transition
has observable consequences across several systems (an API, a database, a message bus). A scenario lets one
test walk that lifecycle and pin down every transition, while still reading like prose.

For a **unit** test — one method, one mocked collaborator — a single AAI triple is usually clearer; reach for
a scenario only when the behaviour genuinely spans several steps.

:::tip[Keep each block honest]
The [produce-vs-observe rule](/concepts/aai/#the-discipline-produce-vs-observe) still holds at every step:
Arrange and Act *produce*, Inspect only *observes*. That's what lets you trust each intermediate check — a
failing Inspect is always reporting on what the preceding Act did, never on something the Inspect caused.
:::
