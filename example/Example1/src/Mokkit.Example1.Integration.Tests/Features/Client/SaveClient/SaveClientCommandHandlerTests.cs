using FluentValidation;
using Mokkit.Inspect;
using Mokkit.Example1.Application.Features.Client.SaveClient;
using Mokkit.Example1.Common;
using Mokkit.Example1.Domain.Entities;
using static Mokkit.Example1.Integration.Tests.Features.Client.SaveClient.ArrangeSaveClient;

namespace Mokkit.Example1.Integration.Tests.Features.Client.SaveClient;

[TestFixture]
public sealed class SaveClientCommandHandlerTests : BaseIntegrationTest
{
    [Test]
    public async Task Create_PersistsClient_UpdatesCache_AndPublishesCreatedEvent()
    {
        // ARRANGE — deterministic clock + id, then a create command over the defaults.
        await Arrange
            .Clock(ArrangeClient.FixedUtcNow)
            .Ids(ArrangeClient.FixedClientId)
            .CreateClientCommand(out var command, WithName("Acme Corporation"));

        // ACT
        var result = await Act(command);

        // INSPECT — assert success, capture the id from the result (guarded non-empty), then observe by it.
        await Inspect
            .SaveResult(result).IsSuccess(ArrangeClient.FixedClientId)
            .Ensure(result, r => r.ClientId, out var clientId)
            .DbClientById(clientId, out var saved, c => Assert.That(c, Is.Not.Null))
            .Verify(saved)
            .CacheUpdated(clientId)
            .EventPublished(clientId, "created");
    }

    [TestCase("not-an-email", TestName = "Create_WhenEmailMalformed_FailsValidation")]
    [TestCase("", TestName = "Create_WhenEmailMissing_FailsValidation")]
    public async Task Create_WhenEmailInvalid_FailsValidation_AndDoesNotTouchInfrastructure(string email)
    {
        // ARRANGE
        await Arrange
            .Clock()
            .Ids()
            .CreateClientCommand(out var command, WithEmail(email));

        // ACT
        var result = await Act(command);

        // INSPECT — failure surfaces a ValidationException and nothing is persisted/published.
        await Inspect
            .SaveResult(result).IsFailure<ValidationException>()
            .NoClientsInDb()
            .CacheNotUpdated()
            .NoEventsPublished();
    }

    [Test]
    public async Task Update_ModifiesClient_PreservesCreatedAt_AndPublishesUpdatedEvent()
    {
        var createdAt = ArrangeClient.FixedUtcNow;
        var updatedAt = ArrangeClient.FixedUtcNow.AddDays(1);

        // ARRANGE — seed an existing client first (await so the capture is populated),
        // then advance the clock and build an update command targeting the captured id.
        await Arrange
            .DbClient(out var existing, c =>
            {
                c.CreatedAt = createdAt;
                c.UpdatedAt = createdAt;
            });

        await Arrange
            .Clock(updatedAt)
            .UpdateClientCommand(out var command, existing.Value!.Id,
                WithName("Renamed Corporation"),
                WithStatus((int)ClientStatus.Suspended));

        // ACT
        var result = await Act(command);

        // INSPECT — fields changed, CreatedAt preserved, UpdatedAt advanced, event published.
        await Inspect
            .SaveResult(result).IsSuccess(existing.Value!.Id)
            .DbClientById(existing.Value!.Id, out var saved, c =>
            {
                Assert.That(c!.Name, Is.EqualTo("Renamed Corporation"));
                Assert.That(c.Status, Is.EqualTo(ClientStatus.Suspended));
                Assert.That(c.CreatedAt, Is.EqualTo(createdAt));
                Assert.That(c.UpdatedAt, Is.EqualTo(updatedAt));
            })
            .Verify(saved)
            .CacheUpdated(existing.Value!.Id)
            .EventPublished(existing.Value!.Id, "updated");
    }

    [Test]
    public async Task Update_WhenClientDoesNotExist_Fails_AndPublishesNoEvent()
    {
        // ARRANGE — an update targeting an id that was never persisted.
        await Arrange
            .UpdateClientCommand(out var command, ArrangeClient.FixedClientId);

        // ACT
        var result = await Act(command);

        // INSPECT
        await Inspect
            .SaveResult(result).IsFailure<InvalidOperationException>()
            .NoEventsPublished();
    }

    private Task<SaveClientCommandResult> Act(SaveClientCommand command)
        => Stage.ExecuteAsync<IRequestHandler<SaveClientCommand, SaveClientCommandResult>, SaveClientCommandResult>(
            handler => handler.Handle(command).AsTask());
}
