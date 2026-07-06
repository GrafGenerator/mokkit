using Mokkit.Example1.Domain.Entities;
using Mokkit.Example1.Infrastructure.Kafka.Consumers;

namespace Mokkit.Example1.Unit.Tests.Messaging.Processor;

/// <summary>
/// Unit tests for the real <c>ClientStatusChangedProcessor</c> (resolved from the stage). It resolves its
/// save handler + publisher from a real DI scope; Mokkit serves the substitutes into that scope. Every
/// step is a business-named arrange/inspect.
/// </summary>
public sealed class ClientStatusChangedProcessorTests : BaseUnitTest<ProcessorFixture>
{
    public ClientStatusChangedProcessorTests(ProcessorFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ValidMessage_UpdatesClient_AndPublishesConfirmation()
    {
        // ARRANGE
        await Arrange
            .IncomingStatusChange(out var message, status: (int)ClientStatus.Suspended)
            .HandlerSucceedsFor(message);

        // ACT
        await Process(message);

        // INSPECT
        await Inspect
            .HandledUpdate(message)
            .ConfirmationPublishedFor(message);
    }

    [Fact]
    public async Task WhenHandlerFails_NoConfirmationPublished()
    {
        // ARRANGE
        await Arrange
            .IncomingStatusChange(out var message)
            .HandlerFailsFor(message);

        // ACT
        await Process(message);

        // INSPECT — the update was attempted but, since it failed, no confirmation is emitted.
        await Inspect
            .HandledUpdate(message)
            .NoConfirmationPublished();
    }

    [Fact]
    public async Task InvalidJson_IsSkipped_WithoutHandlingOrPublishing()
    {
        // ACT — malformed payload must not throw or trigger downstream work.
        await Process("{ not-valid-json");

        // INSPECT
        await Inspect
            .NotHandled()
            .NoConfirmationPublished();
    }

    [Fact]
    public async Task NullContactFields_MapToEmptyStrings()
    {
        // ARRANGE
        await Arrange
            .IncomingStatusChangeMissingContact(out var message, (int)ClientStatus.Inactive)
            .HandlerSucceedsFor(message);

        // ACT
        await Process(message);

        // INSPECT
        await Inspect.HandledWithEmptyContact(message);
    }

    private Task Process(Capture<ClientStatusChangedMessage> message) =>
        Process(KafkaMessageFaker.ToJson(message.Value!));

    private Task Process(string json) =>
        Stage.ExecuteAsync<IClientStatusChangedProcessor>(processor => processor.ProcessAsync(json));
}
