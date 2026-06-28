using System.Net;
using System.Net.Http.Json;
using Mokkit.Arrange;
using Mokkit.Example1.Domain.Entities;
using Mokkit.Example1.E2E.Tests.Contracts;
using Capture = Mokkit.Capture;

namespace Mokkit.Example1.E2E.Tests.Clients;

/// <summary>Applies one field change over a default <see cref="SaveClientRequest"/>.</summary>
public delegate SaveClientRequest ClientFieldFn(SaveClientRequest request);

/// <summary>
/// High-level client arranges — each performs a real operation against the running service.
/// </summary>
public static class ArrangeClientApi
{
    /// <summary>Creates a client through the real <c>POST /api/v1/clients</c> and captures its id.</summary>
    public static ITestArrange NewClient(
        this ITestArrange arrange, out Capture<Guid> idCapture, params ClientFieldFn[] fields)
    {
        var capture = Capture.Start(out idCapture);
        return arrange.Then(async host =>
        {
            await host.ExecuteAsync<HttpClient>(async http =>
            {
                var response = await http.PostAsJsonAsync("/api/v1/clients", Build(fields));
                response.StatusCode.ShouldBe(HttpStatusCode.Created);
                var body = await response.Content.ReadFromJsonAsync<SaveClientResponse>();
                capture.Set(body!.ClientId);
            });
        });
    }

    /// <summary>Builds a request from sensible, valid defaults plus the given field overrides.</summary>
    public static SaveClientRequest Build(params ClientFieldFn[] fields) =>
        fields.Aggregate(Defaults(), (request, field) => field(request));

    public static ClientFieldFn WithName(string name) => r => r with { Name = name };

    public static ClientFieldFn WithEmail(string email) => r => r with { Email = email };

    public static ClientFieldFn WithPhone(string phone) => r => r with { Phone = phone };

    public static ClientFieldFn WithStatus(ClientStatus status) => r => r with { Status = (int)status };

    private static SaveClientRequest Defaults() => new()
    {
        Name = "Default Co",
        Email = "default@acme.test",
        Phone = "+15555550100",
        Status = (int)ClientStatus.Active
    };
}
