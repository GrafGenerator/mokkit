---
title: Async / eventually-consistent assertions
description: Assert on outcomes that arrive asynchronously — a consumer applying a change, an event landing on a topic — by polling until they're true.
---

Some effects don't happen synchronously with the act. A Kafka message is consumed on a background loop; a
projection updates a moment later; an event lands on a topic when the broker gets around to it. Asserting
*immediately* would be flaky. The fix isn't `Thread.Sleep` — it's an inspect verb that **polls until the
expected state, with a timeout**.

## An "eventually" inspect verb

Compare the immediate read with the polling one. The immediate verb expects the outcome *now*:

```csharp
public static ITestInspect ApiClient(this ITestInspect inspect, Guid clientId, Action<ClientResponse> assert) =>
    inspect.Then(async host => await host.ExecuteAsync<HttpClient>(async http =>
    {
        var response = await http.GetAsync($"/api/v1/clients/{clientId}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        assert((await response.Content.ReadFromJsonAsync<ClientResponse>())!);
    }));
```

The eventually verb re-reads until a predicate holds, and fails with the last thing it saw:

```csharp
private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

public static ITestInspect ApiClientEventually(
    this ITestInspect inspect, Guid clientId, Func<ClientResponse, bool> until) =>
    inspect.Then(async host => await host.ExecuteAsync<HttpClient>(async http =>
    {
        ClientResponse? last = null;
        var reached = await Poll.Until(async () =>
        {
            var response = await http.GetAsync($"/api/v1/clients/{clientId}");
            if (response.StatusCode != HttpStatusCode.OK) return false;
            last = await response.Content.ReadFromJsonAsync<ClientResponse>();
            return last is not null && until(last);
        }, Timeout);

        reached.ShouldBeTrue(
            $"client {clientId} did not reach the expected state within {Timeout.TotalSeconds:0}s (last seen: {last})");
    }));
```

`Poll.Until(predicate, timeout)` is a tiny helper: it calls the async predicate on a short interval and returns
`true` as soon as it succeeds, or `false` at the deadline. Capturing `last` means a failure tells you *what the
system actually looked like*, not just "timed out" — the difference between a five-minute debug and a five-second
one.

## Waiting for an event

The same idea applies to messages. The `EventPublished` verb waits up to a timeout for the probe to see a
message keyed by the client id, rather than assuming it's already there:

```csharp
public static ITestInspect EventPublished(this ITestInspect inspect, string topic, Guid clientId) =>
    inspect.Then(async host => await host.ExecuteAsync<KafkaProbe>(async probe =>
        (await probe.SawMessageKeyed(topic, clientId.ToString(), Timeout))
            .ShouldBeTrue($"expected a message on '{topic}' keyed by {clientId}")));
```

## Using it

Pick the eventually verb for anything the system does *after* the act returns — here, a status change applied by
a Kafka consumer:

```csharp
// ACT — emit the status-changed message (a void act; the consumer picks it up asynchronously).
await Act.ProduceStatusChanged(clientId, message);

// INSPECT — the change shows up via the API eventually, is persisted, and a confirmation is published.
await Inspect
    .ApiClientEventually(clientId, c => c.Status == (int)ClientStatus.Suspended)
    .DbClient(clientId, c => c!.Status.ShouldBe(ClientStatus.Suspended))
    .EventPublished("clients.updated", clientId);
```

:::tip[Keep synchronous checks synchronous]
Only reach for polling where the outcome is genuinely asynchronous. A value the act already produced (a returned
result, a row the handler wrote inline) should be asserted immediately — polling there just hides bugs behind a
timeout. Match the verb to the timing.
:::

## Next

- **[Full black-box E2E with Testcontainers](/guides/end-to-end/)** — where async effects are everywhere.
- **[Scenario tests](/concepts/scenarios/)** — interleave eventually-checks between steps of a lifecycle.
- **[Parallel inspects with ThenAll](/guides/thenall/)** — observe several async effects concurrently.
