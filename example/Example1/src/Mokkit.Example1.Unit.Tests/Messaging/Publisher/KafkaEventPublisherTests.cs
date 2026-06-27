using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Mokkit.Example1.Infrastructure.Logic.Events;

namespace Mokkit.Example1.Unit.Tests.Messaging.Publisher;

/// <summary>
/// Unit tests for the real <c>KafkaEventPublisher</c> with the actual Confluent
/// <see cref="IProducer{TKey,TValue}"/> faked. Demonstrates the <b>construct-in-Act</b> style: the Act
/// helpers new up the SUT but resolve its dependencies (producer + logger) from the stage.
/// </summary>
public sealed class KafkaEventPublisherTests : BaseUnitTest<PublisherFixture>
{
    public KafkaEventPublisherTests(PublisherFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Publish_SendsToEventTopic_WithClientKeyAndPayload()
    {
        // ARRANGE
        var clientId = Guid.NewGuid();
        await Arrange.ProducerAcceptsMessages();

        // ACT
        await Publish(clientId, "updated");

        // INSPECT
        await Inspect.PublishedEvent(clientId, "updated");
    }

    [Fact]
    public async Task Publish_WhenProducerThrows_Rethrows()
    {
        // ARRANGE
        await Arrange.ProducerIsDown<InvalidOperationException>();

        // ACT
        var error = await PublishCapturingError(Guid.NewGuid(), "updated");

        // INSPECT
        await Inspect.Faulted<InvalidOperationException>(error);
    }

    private Task Publish(Guid clientId, string eventType) =>
        Stage.ExecuteAsync<IProducer<string, string>, ILogger<KafkaEventPublisher>>(
            (producer, logger) => new KafkaEventPublisher(producer, logger).PublishClientEventAsync(clientId, eventType));

    private async Task<Exception?> PublishCapturingError(Guid clientId, string eventType)
    {
        Exception? captured = null;

        await Stage.ExecuteAsync<IProducer<string, string>, ILogger<KafkaEventPublisher>>(async (producer, logger) =>
        {
            try
            {
                await new KafkaEventPublisher(producer, logger).PublishClientEventAsync(clientId, eventType);
            }
            catch (Exception ex)
            {
                captured = ex;
            }
        });

        return captured;
    }
}
