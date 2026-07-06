using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Mokkit.Example1.Infrastructure.Kafka.Consumers;

namespace Mokkit.Example1.Unit.Tests.Messaging.Consumer;

/// <summary>
/// Unit tests for the consumer's single-poll step with the actual Confluent
/// <see cref="IConsumer{TKey,TValue}"/> and the processor faked. Demonstrates the <b>construct-in-Act</b>
/// style: Act news up the SUT but resolves consumer + processor + logger from the stage.
/// </summary>
public sealed class ClientEventConsumerTests : BaseUnitTest<ConsumerFixture>
{
    public ClientEventConsumerTests(ConsumerFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ConsumeOnce_WhenMessageAvailable_ProcessesAndCommits()
    {
        // ARRANGE
        await Arrange.MessageAvailable(out var message);

        // ACT
        var processed = await ConsumeOnce();

        // INSPECT
        await Inspect
            .MessageConsumed(processed)
            .ForwardedToProcessor(message)
            .OffsetCommitted(message);
    }

    [Fact]
    public async Task ConsumeOnce_WhenNoMessage_DoesNothing()
    {
        // ARRANGE
        await Arrange.NoMessageAvailable();

        // ACT
        var processed = await ConsumeOnce();

        // INSPECT
        await Inspect
            .NoMessageConsumed(processed)
            .NothingForwarded()
            .NothingCommitted();
    }

    private Task<bool> ConsumeOnce() =>
        Stage.ExecuteAsync<IConsumer<string, string>, IClientStatusChangedProcessor, ILogger<ClientEventConsumer>, bool>(
            (consumer, processor, logger) =>
                new ClientEventConsumer(consumer, processor, logger).ConsumeOnceAsync(CancellationToken.None));
}
