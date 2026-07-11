using Mokkit;
using Mokkit.Arrange;
using Mokkit.Example1.E2E.Tests.Contracts;

namespace Mokkit.Example1.E2E.Tests.Clients;

/// <summary>
/// Arrange helpers whose bodies are supplied by the <c>[MokkitCapture]</c> source generator — no hand-written
/// <c>Trapture.Start(...)</c> / <c>Then(Set(new ...))</c> boilerplate. The generator builds the message from the
/// method's parameters (object-initializer here, since <see cref="StatusChangedMessage"/> uses init properties).
/// </summary>
public static partial class ArrangeMessages
{
    [MokkitCapture]
    public static partial ITestArrange StatusChanged(
        this ITestArrange arrange,
        out Trapture<StatusChangedMessage> message,
        Guid clientId,
        string? name,
        string? email,
        string? phone,
        int status);
}
