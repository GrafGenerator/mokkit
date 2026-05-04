using Mokkit.Example1.Application.Features.Client.GetClient;
using Mokkit.Inspect;

namespace Mokkit.Example1.Integration.Tests.Features.Client.GetClient;

/// <summary>
/// Inspect building blocks for <see cref="GetClientQueryResult"/>.
/// </summary>
public static class InspectGetClient
{
    /// <summary>
    /// Opens a value scope over the result so chained assertions run inside a single
    /// <c>Assert.Multiple</c> block.
    /// </summary>
    public static ITestInspectScope<GetClientQueryResult> GetResult(
        this ITestInspect inspect,
        GetClientQueryResult result)
    {
        return inspect.ThenValueScope(result, async (_, execute) =>
        {
            await Assert.MultipleAsync(async () => await execute());
        });
    }

    /// <summary>Asserts the client was found and has the expected id.</summary>
    public static ITestInspectScope<GetClientQueryResult> Found(
        this ITestInspectScope<GetClientQueryResult> inspect,
        Guid expectedClientId)
    {
        return inspect.Then((result, _) =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Client, Is.Not.Null);
            Assert.That(result.Client!.Id, Is.EqualTo(expectedClientId));
        });
    }

    /// <summary>Asserts the client was not found.</summary>
    public static ITestInspectScope<GetClientQueryResult> NotFound(
        this ITestInspectScope<GetClientQueryResult> inspect)
    {
        return inspect.Then((result, _) =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Client, Is.Null);
        });
    }
}
