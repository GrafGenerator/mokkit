using Mokkit;
using Mokkit.Arrange;
using Mokkit.Example1.Application.Features.Client.SaveClient;
using Mokkit.Example1.Common;
using Mokkit.Example1.Infrastructure.Kafka.Consumers;
using Capture = Mokkit.Capture;

namespace Mokkit.Example1.Unit.Tests.Messaging.Processor;

/// <summary>
/// Arrange building blocks for the status-changed processor — build (and capture) the incoming message and
/// configure the substituted save handler the processor resolves from its DI scope.
/// </summary>
internal static partial class ArrangeProcessor
{
    public static ITestArrange IncomingStatusChange(
        this ITestArrange arrange,
        out Capture<ClientStatusChangedMessage> messageCapture,
        int? status = null)
    {
        var capture = Capture.Start(out messageCapture);
        return arrange.Then(_ => capture.Set(KafkaMessageFaker.NewStatusChanged(status: status)));
    }

    // Body supplied by the [MokkitCapture] source generator: it object-initializes the message from the
    // parameters (ClientId, Status) and leaves the unmentioned contact fields (Name/Email/Phone) null.
    [MokkitCapture]
    public static partial ITestArrange IncomingStatusChangeMissingContact(
        this ITestArrange arrange,
        out Capture<ClientStatusChangedMessage> messageCapture,
        Guid clientId,
        int status);

    public static ITestArrange HandlerSucceedsFor(
        this ITestArrange arrange,
        Capture<ClientStatusChangedMessage> message)
    {
        return arrange.Then(host =>
        {
            host.Execute<IRequestHandler<SaveClientCommand, SaveClientCommandResult>>(handler =>
                handler.Handle(Arg.Any<SaveClientCommand>(), Arg.Any<CancellationToken>())
                    .Returns(new SaveClientCommandResult(true, message.Value!.ClientId)));
        });
    }

    public static ITestArrange HandlerFailsFor(
        this ITestArrange arrange,
        Capture<ClientStatusChangedMessage> message)
    {
        return arrange.Then(host =>
        {
            host.Execute<IRequestHandler<SaveClientCommand, SaveClientCommandResult>>(handler =>
                handler.Handle(Arg.Any<SaveClientCommand>(), Arg.Any<CancellationToken>())
                    .Returns(new SaveClientCommandResult(false, message.Value!.ClientId,
                        new InvalidOperationException("update failed"))));
        });
    }
}
