using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Mokkit.Example1.Db.Postgres;
using Mokkit.Example1.Domain.Entities;
using Mokkit.Example1.E2E.Tests.Contracts;
using Mokkit.Inspect;

namespace Mokkit.Example1.E2E.Tests.Clients;

/// <summary>
/// Inspects that read outcomes from the running system — the public API, the database, and Kafka.
/// </summary>
public static class InspectClientApi
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    /// <summary>Fetches the client via <c>GET</c> (expects 200) and asserts on it.</summary>
    public static ITestInspect ApiClient(this ITestInspect inspect, Guid clientId, Action<ClientResponse> assert) =>
        inspect.Then(async host => await host.ExecuteAsync<HttpClient>(async http =>
        {
            var response = await http.GetAsync($"/api/v1/clients/{clientId}");
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<ClientResponse>();
            assert(body!);
        }));

    /// <summary>Polls <c>GET</c> until the client satisfies <paramref name="until"/> (for async outcomes).</summary>
    public static ITestInspect ApiClientEventually(this ITestInspect inspect, Guid clientId, Func<ClientResponse, bool> until) =>
        inspect.Then(async host => await host.ExecuteAsync<HttpClient>(async http =>
        {
            ClientResponse? last = null;
            var reached = await Poll.Until(async () =>
            {
                var response = await http.GetAsync($"/api/v1/clients/{clientId}");
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return false;
                }

                last = await response.Content.ReadFromJsonAsync<ClientResponse>();
                return last is not null && until(last);
            }, Timeout);

            reached.ShouldBeTrue($"client {clientId} did not reach the expected state within {Timeout.TotalSeconds:0}s (last seen: {last})");
        }));

    /// <summary>Asserts <c>GET</c> returns 404.</summary>
    public static ITestInspect ApiClientNotFound(this ITestInspect inspect, Guid clientId) =>
        inspect.Then(async host => await host.ExecuteAsync<HttpClient>(async http =>
        {
            var response = await http.GetAsync($"/api/v1/clients/{clientId}");
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }));

    /// <summary>Opens a value scope over the write-result artifact handed over by the Act.</summary>
    public static ITestInspectScope<ClientWriteResult> WriteResult(this ITestInspect inspect, ClientWriteResult result) =>
        inspect.ThenValueScope(result);

    /// <summary>Asserts the create succeeded: 201 Created with a real client id.</summary>
    public static ITestInspectScope<ClientWriteResult> Created(this ITestInspectScope<ClientWriteResult> inspect) =>
        inspect.Then((result, _) =>
        {
            result.Status.ShouldBe(HttpStatusCode.Created);
            result.ClientId.ShouldNotBeNull();
        });

    /// <summary>Asserts the update succeeded: 200 OK.</summary>
    public static ITestInspectScope<ClientWriteResult> Updated(this ITestInspectScope<ClientWriteResult> inspect) =>
        inspect.Then((result, _) => result.Status.ShouldBe(HttpStatusCode.OK));

    /// <summary>Asserts the write was rejected: 400 Bad Request, no client id.</summary>
    public static ITestInspectScope<ClientWriteResult> Rejected(this ITestInspectScope<ClientWriteResult> inspect) =>
        inspect.Then((result, _) =>
        {
            result.Status.ShouldBe(HttpStatusCode.BadRequest);
            result.ClientId.ShouldBeNull();
        });

    /// <summary>Reads the row straight from Postgres and asserts on it.</summary>
    public static ITestInspect DbClient(this ITestInspect inspect, Guid clientId, Action<Client?> assert) =>
        inspect.Then(async host => await host.ExecuteAsync<ExampleContext>(async db =>
            assert(await db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clientId))));

    /// <summary>Confirms the service published an event keyed by the client id on the given topic.</summary>
    public static ITestInspect EventPublished(this ITestInspect inspect, string topic, Guid clientId) =>
        inspect.Then(async host => await host.ExecuteAsync<KafkaProbe>(async probe =>
            (await probe.SawMessageKeyed(topic, clientId.ToString(), Timeout))
                .ShouldBeTrue($"expected a message on '{topic}' keyed by {clientId}")));
}
