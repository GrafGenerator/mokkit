using Mokkit.Example1.Domain.Entities;
using static Mokkit.Example1.E2E.Tests.Clients.ArrangeClientApi;

namespace Mokkit.Example1.E2E.Tests.Clients;

/// <summary>
/// A <b>scenario</b> test — one story told as a sequence of Arrange / Act / Inspect blocks rather than a single
/// AAA triple. A client is created, renamed through the API, then suspended by an upstream status-changed
/// message, and after every step we confirm the whole system reflects it. The captured id threads through the
/// whole story (a <c>Trapture&lt;Guid&gt;</c> that converts transparently), and both Act flavors appear:
/// <see cref="ActClientApi.UpdateClient"/> returns its artifact, <see cref="ActClientApi.ProduceStatusChanged"/>
/// is a void act whose effects are observed downstream.
/// </summary>
[Collection(E2ECollection.Name)]
public sealed class ClientLifecycleScenarioTests : BaseE2ETest
{
    private const string OriginalName = "Acme Corporation";
    private const string RenamedName = "Acme Holdings";
    private const string Email = "lifecycle@e2e.test";
    private const string Phone = "+15555550123";

    public ClientLifecycleScenarioTests(E2EStack stack) : base(stack)
    {
    }

    [Fact]
    public async Task Client_IsCreated_Renamed_ThenSuspended_AndEveryStepIsReflected()
    {
        // ── Born ── create the client (precondition) and confirm it starts out Active.
        await Arrange
            .NewClient(out var clientId, WithName(OriginalName), WithEmail(Email), WithPhone(Phone),
                WithStatus(ClientStatus.Active));

        await Inspect
            .ApiClient(clientId, c =>
            {
                c.Name.ShouldBe(OriginalName);
                c.Status.ShouldBe((int)ClientStatus.Active);
            });

        // ── Renamed ── update it through the API. This act returns its write-result artifact, which the very
        // next Inspect asserts before observing the reflected change.
        var renamed = await Act
            .UpdateClient(clientId, WithName(RenamedName), WithEmail(Email), WithPhone(Phone),
                WithStatus(ClientStatus.Active));

        await Inspect
            .WriteResult(renamed).Updated()
            .ApiClient(clientId, c => c.Name.ShouldBe(RenamedName));

        // ── Suspended ── an upstream system emits a status-changed message carrying the (renamed) record. This
        // act is void — its effects surface eventually via the API, the DB row, and a confirmation event.
        await Arrange
            .ArrangeStatusChanged(out var suspend, clientId, RenamedName, Email, Phone, (int)ClientStatus.Suspended);

        await Act
            .ProduceStatusChanged(clientId, suspend);

        await Inspect
            .ApiClientEventually(clientId, c => c.Status == (int)ClientStatus.Suspended)
            .DbClient(clientId, c =>
            {
                c!.Name.ShouldBe(RenamedName);
                c.Status.ShouldBe(ClientStatus.Suspended);
            })
            .EventPublished("clients.updated", clientId);
    }
}
