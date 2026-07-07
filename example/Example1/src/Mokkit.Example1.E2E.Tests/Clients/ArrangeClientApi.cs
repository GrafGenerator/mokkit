using System.Net;
using Mokkit;
using Mokkit.Arrange;
using Mokkit.Example1.Domain.Entities;
using Mokkit.Example1.E2E.Tests.Contracts;

namespace Mokkit.Example1.E2E.Tests.Clients;

/// <summary>Applies one field change over a default <see cref="SaveClientRequest"/>.</summary>
public delegate SaveClientRequest ClientFieldFn(SaveClientRequest request);

/// <summary>
/// High-level client arranges — each performs a real operation against the running service.
/// </summary>
public static class ArrangeClientApi
{
    /// <summary>
    /// Creates a client through the real <c>POST /api/v1/clients</c> as a <b>precondition</b> and captures
    /// its id (the artifact later steps observe by). The status check here is a setup guard — fail fast if
    /// the precondition couldn't be established — not the assertion under test.
    /// </summary>
    public static ITestArrange NewClient(
        this ITestArrange arrange, out Trapture<Guid> idCapture, params ClientFieldFn[] fields)
    {
        var capture = Trapture.Start(out idCapture);
        return arrange.Then(async host =>
        {
            await host.ExecuteAsync<HttpClient>(async http =>
            {
                var result = await ClientApi.CreateAsync(http, Build(fields));
                result.Status.ShouldBe(HttpStatusCode.Created);
                capture.Set(result.ClientId!.Value);
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
