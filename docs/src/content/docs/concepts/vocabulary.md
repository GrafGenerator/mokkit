---
title: Building your test vocabulary
description: The idea Mokkit is built around — reusable Arrange/Inspect verbs that become your domain's testing language.
---

This is the page that matters most. Everything else in Mokkit exists to support one practice:

> **You author a vocabulary of verbs in your domain's language, and every test is a short composition of
> them.**

The verbs are plain C# extension methods on `ITestArrange` and `ITestInspect`. Once you have a handful, a
test stops looking like plumbing and starts reading like the scenario it describes.

## A test is a composition

Here's a test written entirely in a client-management vocabulary:

```csharp
await Arrange
    .NewClient(out var clientId, WithName("Acme Corporation"), WithEmail("acme@e2e.test"));

var result = await Act
    .UpdateClient(clientId, WithName("Renamed Corporation"));

await Inspect
    .WriteResult(result).Updated()
    .ApiClient(clientId, c => c.Name.ShouldBe("Renamed Corporation"))
    .EventPublished("clients.updated", clientId);
```

`NewClient`, `WithName`, `UpdateClient`, `WriteResult`, `Updated`, `ApiClient`, `EventPublished` aren't Mokkit
APIs — they're **your** methods. Mokkit provides `Arrange` / `Act` / `Inspect` and the machinery underneath;
you provide the words.

## Three kinds of verb

### Arrange verbs — set up, and capture

An arrange verb registers a deferred step and usually hands back a **capture** for the artifact it creates,
so later steps can refer to it:

```csharp
public static ITestArrange NewClient(
    this ITestArrange arrange, out Trapture<Guid> id, params ClientFieldFn[] fields)
{
    var capture = Trapture.Start(out id);
    return arrange.Then(async host =>
    {
        await host.ExecuteAsync<HttpClient>(async http =>
        {
            var result = await ClientApi.CreateAsync(http, Build(fields));
            result.Status.ShouldBe(HttpStatusCode.Created);   // precondition guard
            capture.Set(result.ClientId!.Value);
        });
    });
}
```

The `out` capture is the trick that lets verbs pass data to each other while everything stays deferred:
`NewClient` returns immediately with an *empty* `clientId`; when the chain is awaited, the step runs and
fills it. See [Capture vs Trapture](/concepts/captures/).

Small parameter helpers (`WithName`, `WithEmail`, …) let a caller compose exactly the setup they need:

```csharp
public static ClientFieldFn WithName(string name)  => r => r with { Name = name };
public static ClientFieldFn WithEmail(string email) => r => r with { Email = email };
```

### Act verbs — do the thing, and maybe return a result

An act verb performs the operation under test. Like an arrange, it can hand an artifact back — either by
**returning** it (`Returning`) or, for a void act, leaving its effects to be observed later in Inspect:

```csharp
// Return flavor — `var result = await Act.UpdateClient(...)`.
public static ITestAct<ClientWriteResult> UpdateClient(
    this ITestAct act, Guid clientId, params ClientFieldFn[] fields) =>
    act.Returning(host => host.ExecuteAsync<HttpClient, ClientWriteResult>(
        http => ClientApi.UpdateAsync(http, clientId, Build(fields))));

// Void flavor — fire the operation; its effects surface downstream in Inspect.
public static ITestAct ProduceStatusChanged(this ITestAct act, Guid clientId, StatusChangedMessage message) =>
    act.Then(host => host.ExecuteAsync<IProducer<string, string>>(
        producer => producer.ProduceAsync("clients.status-changed", Serialize(clientId, message))));
```

Act verbs are what let a test grow from a single triple into a [scenario](/concepts/scenarios/) — a sequence
of Arrange / Act / Inspect blocks that walks a whole lifecycle.

### Inspect verbs — observe

An inspect verb resolves what it needs from the stage and asserts. It only reads:

```csharp
public static ITestInspect ApiClient(
    this ITestInspect inspect, Guid clientId, Action<ClientResponse> assert) =>
    inspect.Then(async host => await host.ExecuteAsync<HttpClient>(async http =>
    {
        var response = await http.GetAsync($"/api/v1/clients/{clientId}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        assert((await response.Content.ReadFromJsonAsync<ClientResponse>())!);
    }));

public static ITestInspect EventPublished(this ITestInspect inspect, string topic, Guid clientId) =>
    inspect.Then(async host => await host.ExecuteAsync<KafkaProbe>(async probe =>
        (await probe.SawMessageKeyed(topic, clientId.ToString())).ShouldBeTrue()));
```

## Why this beats a DSL

Because your vocabulary is *code*, you get everything a Gherkin step binding gives up (see
[Why Mokkit?](/why-mokkit/)):

- **Autocomplete.** Type `Inspect.` and your project's assertions are right there.
- **Go-to-definition & debugging.** Step into `EventPublished` — no binding layer in between.
- **Rename & find-usages.** Refactor a verb and every test that uses it updates; the ones that don't fit the
  new signature stop compiling.
- **Typed parameters.** `Guid clientId`, not a string parsed out of a sentence.
- **Provably-correct tests.** `dotnet build` is a real check that the vocabulary is wired up — a
  nonsensical test can't even compile, let alone reach a runner.

The vocabulary is an asset that compounds. The first test costs a few verbs; the tenth reuses them and reads
in seconds.

## Where verbs live

Keep vocabulary next to what it describes — colocated `Arrange<Feature>.cs` / `Inspect<Feature>.cs` files per
feature or system-under-test. The [project structure](/reference/project-structure/) page shows the layout;
the [guides](/quickstart/) build real vocabulary for mocked services, databases, message queues and full
end-to-end flows.

## Levelling up your verbs

As scenarios get richer, a few Mokkit features become vocabulary-authoring techniques rather than test-body
noise:

- **[Value & context scopes](/concepts/aai/)** — group assertions over one value inside a verb.
- **`Ensure`** — derive, guard-as-non-empty, and capture a value in one step, so ids flow cleanly between
  verbs.
- **`[MokkitCapture]`** — let the source generator write the boilerplate body of a "build this object"
  arrange verb, so your vocabulary file is just declarations.

Each is covered in its own guide.
