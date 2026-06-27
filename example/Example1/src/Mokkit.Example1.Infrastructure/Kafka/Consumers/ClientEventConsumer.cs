using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mokkit.Example1.Infrastructure.Kafka.Consumers;

internal sealed class ClientEventConsumer : BackgroundService
{
    private const string Topic = "clients.status-changed";

    private readonly IConsumer<string, string> _consumer;
    private readonly IClientStatusChangedProcessor _processor;
    private readonly ILogger<ClientEventConsumer> _logger;

    public ClientEventConsumer(
        IConsumer<string, string> consumer,
        IClientStatusChangedProcessor processor,
        ILogger<ClientEventConsumer> logger)
    {
        _consumer = consumer;
        _processor = processor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _consumer.Subscribe(new[] { Topic });
            _logger.LogInformation("Kafka consumer started, subscribed to: {Topic}", Topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConsumeOnceAsync(stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message: {Error}", ex.Error.Reason);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Kafka consumer");
        }
        finally
        {
            _consumer.Close();
        }
    }

    /// <summary>
    /// Consumes a single message (if available within the poll window), processes it, and commits.
    /// Returns <c>true</c> when a message was processed. Exposed for unit testing the consume loop
    /// without the infinite <see cref="ExecuteAsync"/> loop.
    /// </summary>
    internal async Task<bool> ConsumeOnceAsync(CancellationToken cancellationToken)
    {
        var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));
        if (consumeResult?.Message == null)
        {
            return false;
        }

        _logger.LogInformation("Processing message from topic {Topic}, partition {Partition}, offset {Offset}",
            consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);

        await _processor.ProcessAsync(consumeResult.Message.Value, cancellationToken);
        _consumer.Commit(consumeResult);

        return true;
    }
}
