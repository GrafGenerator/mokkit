---
title: Test a Kafka consumer / producer
description: Cover message-driven code at every level — the consumer's logic as a unit, and the real produce/consume round-trip end-to-end.
---

Message-driven code has two things worth testing: the **logic** that runs when a message arrives (map it,
dispatch it, publish a confirmation), and the **round-trip** through a real broker. Mokkit handles both — the
first as a fast unit test, the second end-to-end — with the same vocabulary style.

## Unit: the consumer's logic

The processing logic doesn't need a real broker — feed it a JSON payload and verify what it did. The
[unit test](/guides/unit-mocked-dependency/) resolves the real processor and substitutes its collaborators:

```csharp
[Fact]
public async Task ValidMessage_UpdatesClient_AndPublishesConfirmation()
{
    // ARRANGE — an incoming message + a handler set to succeed.
    await Arrange
        .IncomingStatusChange(out var message, status: (int)ClientStatus.Suspended)
        .HandlerSucceedsFor(message);

    // ACT — run the real processor over the raw payload.
    await Stage.Act().Then(host =>
        host.ExecuteAsync<IClientStatusChangedProcessor>(p => p.ProcessAsync(KafkaMessageFaker.ToJson(message.Value!))));

    // INSPECT — it dispatched an Update and published the confirmation.
    await Inspect
        .HandledUpdate(message)
        .ConfirmationPublishedFor(message);
}
```

Malformed input is just another test — `Process("{ not-valid-json")` then `.NotHandled().NoConfirmationPublished()`.

## Producing: a void Act verb

For the real round-trip, producing a message is a **void** [Act](/concepts/aai/#act) — its effect is observed
downstream, so it returns nothing:

```csharp
public static ITestAct ProduceStatusChanged(this ITestAct act, Guid clientId, StatusChangedMessage message) =>
    act.Then(host => host.ExecuteAsync<IProducer<string, string>>(async producer =>
    {
        await producer.ProduceAsync("clients.status-changed", new Message<string, string>
        {
            Key = clientId.ToString(),
            Value = JsonSerializer.Serialize(message)
        });
        producer.Flush(TimeSpan.FromSeconds(5));
    }));
```

## Consuming to assert: a probe

To prove the service *published* an event, read the topic back with a small host-side consumer — a "probe" —
and expose it as an inspect verb. The probe reads from the beginning with a throwaway group and polls up to a
timeout for a message by key:

```csharp
public sealed class KafkaProbe(string bootstrapServers)
{
    public Task<bool> SawMessageKeyed(string topic, string key, TimeSpan timeout) => Task.Run(() =>
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = $"e2e-probe-{Guid.NewGuid():N}",   // throwaway group → read from the start
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topic);

        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
            if (consumer.Consume(TimeSpan.FromMilliseconds(500))?.Message?.Key == key)
                return true;
        return false;
    });
}

// The inspect verb over it:
public static ITestInspect EventPublished(this ITestInspect inspect, string topic, Guid clientId) =>
    inspect.Then(async host => await host.ExecuteAsync<KafkaProbe>(async probe =>
        (await probe.SawMessageKeyed(topic, clientId.ToString(), Timeout)).ShouldBeTrue()));
```

## End-to-end: the whole round-trip

Put them together and one test proves a message flows in, is consumed, applied, and confirmed — all against a
real broker (see [Full E2E](/guides/end-to-end/)):

```csharp
await Arrange.StatusChanged(out var message, clientId, name, email, phone, (int)ClientStatus.Suspended);

await Act.ProduceStatusChanged(clientId, message);       // produce onto the topic

await Inspect
    .ApiClientEventually(clientId, c => c.Status == (int)ClientStatus.Suspended)  // consumed & applied
    .EventPublished("clients.updated", clientId);          // confirmation published
```

:::note[Pre-create topics; auto-create is racy]
The E2E stack creates topics before the API starts so the consumer binds immediately, and the probe polls
rather than assuming the message is already there — message effects are
[eventually consistent](/guides/eventually-consistent/).
:::

## Next

- **[Async / eventually-consistent assertions](/guides/eventually-consistent/)** — why the probe polls.
- **[Full black-box E2E](/guides/end-to-end/)** — the broker running in Docker.
