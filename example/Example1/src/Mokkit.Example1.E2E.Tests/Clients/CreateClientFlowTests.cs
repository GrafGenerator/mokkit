using Mokkit.Example1.Domain.Entities;
using Mokkit.Inspect;
using static Mokkit.Example1.E2E.Tests.Clients.ArrangeClientApi;

namespace Mokkit.Example1.E2E.Tests.Clients;

[Collection(E2ECollection.Name)]
public sealed class CreateClientFlowTests : BaseE2ETest
{
    public CreateClientFlowTests(E2EStack stack) : base(stack)
    {
    }

    [Fact]
    public async Task Create_ViaApi_IsRetrievable_Persisted_AndAnnounced()
    {
        // ACT — creating the client is the action under test; it yields the write-result artifact
        var result = await Act(WithName("Acme Corporation"), WithEmail("acme@e2e.test"));

        // INSPECT — assert the result, capture its id once (guarded non-empty), then observe the three
        // independent downstream effects concurrently: the API read, the DB row and the published event.
        await Inspect
            .WriteResult(result).Created()
            .Ensure(result, r => r.ClientId, out var clientId)
            .ThenAll(
                b => b.ApiClient(clientId, c =>
                {
                    c.Name.ShouldBe("Acme Corporation");
                    c.Email.ShouldBe("acme@e2e.test");
                    c.Status.ShouldBe((int)ClientStatus.Active);
                }),
                b => b.DbClient(clientId, c => c.ShouldNotBeNull()),
                b => b.EventPublished("clients.created", clientId));
    }

    [Fact]
    public async Task Create_WithInvalidEmail_IsRejected()
    {
        // ACT
        var result = await Act(WithEmail("not-an-email"));

        // INSPECT
        await Inspect.WriteResult(result).Rejected();
    }

    [Fact]
    public async Task Get_UnknownClient_Returns404() =>
        await Inspect.ApiClientNotFound(Guid.NewGuid());

    private Task<ClientWriteResult> Act(params ClientFieldFn[] fields) =>
        Stage.ExecuteAsync<HttpClient, ClientWriteResult>(http => ClientApi.CreateAsync(http, Build(fields)));
}
