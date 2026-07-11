using Mokkit.Act;
using Mokkit.Example1.Application.Features.Client.SaveClient;
using Mokkit.Example1.Common;

namespace Mokkit.Example1.Integration.Tests.Features.Client.SaveClient;

/// <summary>
/// The <b>act</b> under test for the save-client feature: dispatching the command through the real handler.
/// Expressed as reusable Act vocabulary (symmetric with <see cref="ArrangeSaveClient"/>), it returns the
/// <see cref="SaveClientCommandResult"/> artifact directly (<c>var r = await Act.SaveClient(command)</c>).
/// </summary>
public static class ActSaveClient
{
    /// <summary>Dispatches the <see cref="SaveClientCommand"/> through its handler and returns the result.</summary>
    public static ITestAct<SaveClientCommandResult> SaveClient(this ITestAct act, SaveClientCommand command) =>
        act.Returning(host =>
            host.ExecuteAsync<IRequestHandler<SaveClientCommand, SaveClientCommandResult>, SaveClientCommandResult>(
                handler => handler.Handle(command).AsTask()));
}
