using Mokkit.Example1.Application.Features.Client.SaveClient;
using Mokkit.Example1.Domain.Entities;

namespace Mokkit.Example1.Unit.Tests.Validation;

/// <summary>
/// Builds <see cref="SaveClientCommand"/> values over sensible, valid defaults for validator tests;
/// each test mutates only the field it is exercising.
/// </summary>
internal static class SaveClientCommands
{
    public static SaveClientCommand Create(Action<SaveClientData>? mutate = null)
    {
        var data = new SaveClientData
        {
            Name = "Acme Corporation",
            Email = "contact@acme.test",
            Phone = "+15555550100",
            Status = (int)ClientStatus.Active
        };

        mutate?.Invoke(data);

        return new SaveClientCommand { Operation = SaveOperationKind.Create, ClientData = data };
    }

    public static SaveClientCommand Update(Guid? id, Action<SaveClientData>? mutate = null)
    {
        var command = Create(mutate);
        command.ClientData.Id = id;

        return command with { Operation = SaveOperationKind.Update };
    }
}
