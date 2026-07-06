using Mokkit.Arrange;
using Mokkit.Example1.Application.Features.Client.SaveClient;
using Capture = Mokkit.Capture;

namespace Mokkit.Example1.Integration.Tests.Features.Client.SaveClient;

/// <summary>
/// Applies a single mutation over the default <see cref="SaveClientData"/>, letting tests
/// compose a command by listing only what differs from the defaults.
/// </summary>
public delegate SaveClientData ClientMutateFn(SaveClientData data);

/// <summary>
/// Arrange building blocks for <see cref="SaveClientCommand"/>.
/// </summary>
public static class ArrangeSaveClient
{
    /// <summary>Builds a <see cref="SaveOperationKind.Create"/> command over the defaults.</summary>
    public static ITestArrange CreateClientCommand(
        this ITestArrange arrange,
        out Capture<SaveClientCommand> commandCapture,
        params ClientMutateFn[] mutateFns)
    {
        return arrange.Command(out commandCapture, SaveOperationKind.Create, clientId: null, mutateFns);
    }

    /// <summary>Builds a <see cref="SaveOperationKind.Update"/> command targeting an existing client.</summary>
    public static ITestArrange UpdateClientCommand(
        this ITestArrange arrange,
        out Capture<SaveClientCommand> commandCapture,
        Guid clientId,
        params ClientMutateFn[] mutateFns)
    {
        return arrange.Command(out commandCapture, SaveOperationKind.Update, clientId, mutateFns);
    }

    private static ITestArrange Command(
        this ITestArrange arrange,
        out Capture<SaveClientCommand> commandCapture,
        SaveOperationKind operation,
        Guid? clientId,
        ClientMutateFn[] mutateFns)
    {
        var capture = Capture.Start(out commandCapture);

        return arrange.Then(_ =>
        {
            var data = new SaveClientData
            {
                Id = clientId,
                Name = ArrangeClient.DefaultName,
                Email = ArrangeClient.DefaultEmail,
                Phone = ArrangeClient.DefaultPhone,
                Status = ArrangeClient.DefaultStatus
            };

            foreach (var mutate in mutateFns)
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
