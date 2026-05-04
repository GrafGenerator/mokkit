using Mokkit.Example1.Application.Features.Client.SaveClient;
using Mokkit.Inspect;

namespace Mokkit.Example1.Integration.Tests.Features.Client.SaveClient;

/// <summary>
/// Inspect building blocks for <see cref="SaveClientCommandResult"/>.
/// </summary>
public static class InspectSaveClient
{
    /// <summary>
    /// Opens a value scope over the result so chained assertions run inside a single
    /// <c>Assert.Multiple</c> block.
    /// </summary>
    public static ITestInspectScope<SaveClientCommandResult> SaveResult(
        this ITestInspect inspect,
        SaveClientCommandResult result)
    {
        return inspect.ThenValueScope(result, async (_, execute) =>
        {
            await Assert.MultipleAsync(async () => await execute());
        });
    }

    /// <summary>Asserts the operation succeeded and, optionally, returned the expected client id.</summary>
    public static ITestInspectScope<SaveClientCommandResult> IsSuccess(
        this ITestInspectScope<SaveClientCommandResult> inspect,
        Guid? expectedClientId = null)
    {
        return inspect.Then((result, _) =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Exception, Is.Null);

            if (expectedClientId.HasValue)
            {
                Assert.That(result.ClientId, Is.EqualTo(expectedClientId.Value));
            }
        });
    }

    /// <summary>Asserts the operation failed with an exception of type <typeparamref name="TException"/>.</summary>
    public static ITestInspectScope<SaveClientCommandResult> IsFailure<TException>(
        this ITestInspectScope<SaveClientCommandResult> inspect)
        where TException : Exception
    {
        return inspect.Then((result, _) =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Exception, Is.InstanceOf<TException>());
        });
    }
}
