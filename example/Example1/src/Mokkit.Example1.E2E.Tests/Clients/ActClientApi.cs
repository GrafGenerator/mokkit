using System.Text.Json;
using Confluent.Kafka;
using Mokkit.Act;
using Mokkit.Example1.E2E.Tests.Contracts;

namespace Mokkit.Example1.E2E.Tests.Clients;

/// <summary>
/// High-level client <b>acts</b> — the operations under test, expressed as reusable Act vocabulary (symmetric
/// with <see cref="ArrangeClientApi"/>). Each drives the real running service. The write acts return their
/// <see cref="ClientWriteResult"/> artifact directly (<c>var r = await Act.CreateClient(...)</c>); the
/// fire-and-observe act returns nothing (its effects are asserted downstream in Inspect).
/// </summary>
public static class ActClientApi
{
    /// <summary>Creates a client through the real <c>POST /api/v1/clients</c> and returns the write result.</summary>
    public static ITestAct<ClientWriteResult> CreateClient(this ITestAct act, params ClientFieldFn[] fields) =>
        act.Returning(host => host.ExecuteAsync<HttpClient, ClientWriteResult>(
            http => ClientApi.CreateAsync(http, ArrangeClientApi.Build(fields))));

    /// <summary>Updates a client through the real <c>PUT /api/v1/clients/{id}</c> and returns the write result.</summary>
    public static ITestAct<ClientWriteResult> UpdateClient(
        this ITestAct act, Guid clientId, params ClientFieldFn[] fields) =>
        act.Returning(host => host.ExecuteAsync<HttpClient, ClientWriteResult>(
            http => ClientApi.UpdateAsync(http, clientId, ArrangeClientApi.Build(fields))));

    /// <summary>
    /// Emits a status-changed message onto Kafka — a void act whose effects (the consumer applying the change)
    /// are observed later in Inspect. The message carries the full record, which the consumer validates.
    /// </summary>
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
}
