using Mokkit.Example1.Application.Features.Client.SaveClient;
using Mokkit.Example1.Application.Logic.Messages;
using Mokkit.Example1.Common;
using Mokkit.Example1.Infrastructure.Kafka.Consumers;
using Mokkit.Inspect;
using Capture = Mokkit.Capture;

namespace Mokkit.Example1.Unit.Tests.Messaging.Processor;

/// <summary>
/// Inspect building blocks that verify how the processor drove the (substituted) save handler and the
/// confirmation publisher.
/// </summary>
internal static class InspectProcessor
{
    /// <summary>Verifies the handler received an Update command mapped from the message.</summary>
    public static ITestInspect HandledUpdate(this ITestInspect inspect, Capture<ClientStatusChangedMessage> message)
    {
        return inspect.Then(host =>
        {
            host.Execute<IRequestHandler<SaveClientCommand, SaveClientCommandResult>>(handler =>
                handler.Received(1).Handle(
                    Arg.Is<SaveClientCommand>(c =>
                        c.Operation == SaveOperationKind.Update &&
                        c.ClientData.Id == message.Value!.ClientId &&
                        c.ClientData.Name == message.Value!.Name &&
                        c.ClientData.Email == message.Value!.Email &&
                        c.ClientData.Phone == message.Value!.Phone &&
                        c.ClientData.Status == message.Value!.Status),
                    Arg.Any<CancellationToken>()));
        });
    }

    /// <summary>Verifies null message fields were mapped to empty strings on the command.</summary>
    public static ITestInspect HandledWithEmptyContact(this ITestInspect inspect, Capture<ClientStatusChangedMessage> message)
    {
        return inspect.Then(host =>
        {
            host.Execute<IRequestHandler<SaveClientCommand, SaveClientCommandResult>>(handler =>
                handler.Received(1).Handle(
                    Arg.Is<SaveClientCommand>(c =>
                        c.ClientData.Id == message.Value!.ClientId &&
                        c.ClientData.Name == string.Empty &&
                        c.ClientData.Email == string.Empty &&
                        c.ClientData.Phone == string.Empty &&
                        c.ClientData.Status == message.Value!.Status),
                    Arg.Any<CancellationToken>()));
        });
    }

    /// <summary>Verifies the handler was never invoked.</summary>
    public static ITestInspect NotHandled(this ITestInspect inspect)
    {
        return inspect.Then(host =>
        {
            host.Execute<IRequestHandler<SaveClientCommand, SaveClientCommandResult>>(handler =>
                handler.DidNotReceive().Handle(Arg.Any<SaveClientCommand>(), Arg.Any<CancellationToken>()));
        });
    }

    /// <summary>Verifies a confirmation event was published for the message's client.</summary>
    public static ITestInspect ConfirmationPublishedFor(this ITestInspect inspect, Capture<ClientStatusChangedMessage> message)
    {
        return inspect.Then(host =>
        {
            host.Execute<IKafkaEventPublisher>(publisher =>
                publisher.Received(1).PublishClientEventAsync(message.Value!.ClientId, "updated", Arg.Any<CancellationToken>()));
        });
    }

    /// <summary>Verifies no confirmation event was published.</summary>
    public static ITestInspect NoConfirmationPublished(this ITestInspect inspect)
    {
        return inspect.Then(host =>
        {
            host.Execute<IKafkaEventPublisher>(publisher =>
                publisher.DidNotReceive().PublishClientEventAsync(
                    Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()));
        });
    }
}
