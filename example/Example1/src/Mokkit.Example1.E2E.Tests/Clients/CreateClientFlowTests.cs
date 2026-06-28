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
        // ARRANGE / ACT — creating the client is the operation under test
        await Arrange
            .NewClient(out var clientId, WithName("Acme Corporation"), WithEmail("acme@e2e.test"));

        // INSPECT — the public API, the database, and the emitted domain event all agree
        await Inspect
            .ApiClient(clientId, c =>
            {
                c.Name.ShouldBe("Acme Corporation");
                c.Email.ShouldBe("acme@e2e.test");
                c.Status.ShouldBe((int)ClientStatus.Active);
            })
            .DbClient(clientId, c => c.ShouldNotBeNull())
            .EventPublished("clients.created", clientId);
    }

    [Fact]
    public async Task Create_WithInvalidEmail_IsRejected() =>
        await Inspect.PostRejected(WithEmail("not-an-email"));

    [Fact]
    public async Task Get_UnknownClient_Returns404() =>
        await Inspect.ApiClientNotFound(Guid.NewGuid());
}
