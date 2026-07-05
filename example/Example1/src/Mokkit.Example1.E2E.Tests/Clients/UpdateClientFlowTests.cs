using Mokkit.Example1.Domain.Entities;
using static Mokkit.Example1.E2E.Tests.Clients.ArrangeClientApi;

namespace Mokkit.Example1.E2E.Tests.Clients;

[Collection(E2ECollection.Name)]
public sealed class UpdateClientFlowTests : BaseE2ETest
{
    public UpdateClientFlowTests(E2EStack stack) : base(stack)
    {
    }

    [Fact]
    public async Task Update_ViaApi_ChangesAreReflected()
    {
        // ARRANGE — an existing client (precondition), confirmed present before we change it
        await Arrange
            .NewClient(out var clientId, WithName("Acme Corporation"), WithStatus(ClientStatus.Active));
        await Inspect
            .ApiClient(clientId, c => c.Name.ShouldBe("Acme Corporation"));

        // ACT — update it; the write yields the result artifact
        var result = await Act(clientId, WithName("Renamed Corporation"), WithStatus(ClientStatus.Suspended));

        // INSPECT — assert the result, then observe the reflected change
        await Inspect
            .WriteResult(result).Updated()
            .ApiClient(clientId, c =>
            {
                c.Name.ShouldBe("Renamed Corporation");
                c.Status.ShouldBe((int)ClientStatus.Suspended);
            })
            .DbClient(clientId, c => c!.Status.ShouldBe(ClientStatus.Suspended));
    }

    private Task<ClientWriteResult> Act(Guid clientId, params ClientFieldFn[] fields) =>
        Stage.ExecuteAsync<HttpClient, ClientWriteResult>(http => ClientApi.UpdateAsync(http, clientId, Build(fields)));
}
