using Mokkit.Example1.Domain.Entities;
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

        // INSPECT — assert the result, then observe downstream effects by its id
        await Inspect
            .WriteResult(result).Created()
            .ApiClient(result.ClientId!.Value, c =>
            {
                c.Name.ShouldBe("Acme Corporation");
                c.Email.ShouldBe("acme@e2e.test");
                c.Status.ShouldBe((int)ClientStatus.Active);
            })
            .DbClient(result.ClientId!.Value, c => c.ShouldNotBeNull())
            .EventPublished("clients.created", result.ClientId!.Value);
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
