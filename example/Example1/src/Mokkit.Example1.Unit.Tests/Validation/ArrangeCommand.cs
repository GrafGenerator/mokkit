using Mokkit;
using Mokkit.Arrange;
using Mokkit.Example1.Application.Features.Client.SaveClient;
using Mokkit.Example1.Domain.Entities;

namespace Mokkit.Example1.Unit.Tests.Validation;

/// <summary>Applies one mutation over the default <see cref="SaveClientData"/>.</summary>
public delegate SaveClientData ClientMutateFn(SaveClientData data);

/// <summary>
/// Arrange building blocks that build (and capture) a <see cref="SaveClientCommand"/> over valid defaults;
/// each test mutates only the field it exercises.
/// </summary>
public static class ArrangeCommand
{
    public static ITestArrange SaveCommand(
        this ITestArrange arrange,
        out Trapture<SaveClientCommand> commandCapture,
        params ClientMutateFn[] mutators) =>
        arrange.Build(out commandCapture, SaveOperationKind.Create, clientId: null, mutators);

    public static ITestArrange UpdateCommand(
        this ITestArrange arrange,
        out Trapture<SaveClientCommand> commandCapture,
        Guid? clientId,
        params ClientMutateFn[] mutators) =>
        arrange.Build(out commandCapture, SaveOperationKind.Update, clientId, mutators);

    private static ITestArrange Build(
        this ITestArrange arrange,
        out Trapture<SaveClientCommand> commandCapture,
        SaveOperationKind operation,
        Guid? clientId,
        ClientMutateFn[] mutators)
    {
        var capture = Trapture.Start(out commandCapture);
        return arrange.Then(_ =>
        {
            var data = new SaveClientData
            {
                Id = clientId,
                Name = "Acme Corporation",
                Email = "contact@acme.test",
                Phone = "+15555550100",
                Status = (int)ClientStatus.Active
            };

            foreach (var mutate in mutators)
            {
                data = mutate(data);
            }

            capture.Set(new SaveClientCommand { Operation = operation, ClientData = data });
        });
    }

    public static ClientMutateFn WithId(Guid? id) => data => data with { Id = id };

    public static ClientMutateFn WithName(string? name) => data => data with { Name = name! };

    public static ClientMutateFn WithEmail(string? email) => data => data with { Email = email! };

    public static ClientMutateFn WithPhone(string? phone) => data => data with { Phone = phone! };

    public static ClientMutateFn WithStatus(int status) => data => data with { Status = status };
}
