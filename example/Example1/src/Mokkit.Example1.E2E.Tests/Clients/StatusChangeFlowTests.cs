using Mokkit.Example1.Domain.Entities;
using static Mokkit.Example1.E2E.Tests.Clients.ArrangeClientApi;

namespace Mokkit.Example1.E2E.Tests.Clients;

[Collection(E2ECollection.Name)]
public sealed class StatusChangeFlowTests : BaseE2ETest
{
    private const string Name = "Acme Corporation";
    private const string Email = "status@e2e.test";
    private const string Phone = "+15555550100";

    public StatusChangeFlowTests(E2EStack stack) : base(stack)
    {
    }

    [Fact]
    public async Task StatusChangedMessage_IsConsumed_AndReflectedEverywhere()
    {
        // ARRANGE — a real client created through the public API, then the status-changed message that will be
        // fed to it. ArrangeStatusChanged's body is source-generated from [MokkitCapture] (see ArrangeMessages).
        await Arrange
            .NewClient(out var clientId, WithName(Name), WithEmail(Email), WithPhone(Phone), WithStatus(ClientStatus.Active));

        await Arrange
            .ArrangeStatusChanged(out var message, clientId, Name, Email, Phone, (int)ClientStatus.Suspended);

        // ACT — an upstream system emits the status-changed message onto Kafka (carrying the full record,
        // which the consumer validates before applying)
        await Act.ProduceStatusChanged(clientId, message);

        // INSPECT — the service consumes it; the change shows up via the API (eventually — it's async),
        // is persisted, and a confirmation event is published
        await Inspect
            .ApiClientEventually(clientId, c => c.Status == (int)ClientStatus.Suspended)
            .DbClient(clientId, c => c!.Status.ShouldBe(ClientStatus.Suspended))
            .EventPublished("clients.updated", clientId);
    }
}
