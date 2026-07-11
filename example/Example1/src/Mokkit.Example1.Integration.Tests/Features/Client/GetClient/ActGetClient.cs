using Mokkit.Act;
using Mokkit.Example1.Application.Features.Client.GetClient;
using Mokkit.Example1.Common;

namespace Mokkit.Example1.Integration.Tests.Features.Client.GetClient;

/// <summary>
/// The <b>act</b> under test for the get-client feature: dispatching the query through the real handler.
/// Expressed as reusable Act vocabulary, it returns the <see cref="GetClientQueryResult"/> artifact directly
/// (<c>var r = await Act.GetClient(query)</c>).
/// </summary>
public static class ActGetClient
{
    /// <summary>Dispatches the <see cref="GetClientQuery"/> through its handler and returns the result.</summary>
    public static ITestAct<GetClientQueryResult> GetClient(this ITestAct act, GetClientQuery query) =>
        act.Returning(host =>
            host.ExecuteAsync<IRequestHandler<GetClientQuery, GetClientQueryResult>, GetClientQueryResult>(
                handler => handler.Handle(query).AsTask()));
}
