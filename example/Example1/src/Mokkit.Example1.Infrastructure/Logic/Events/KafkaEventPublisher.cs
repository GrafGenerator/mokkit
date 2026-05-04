using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mokkit.Example1.Application.Logic.Messages;
using Mokkit.Example1.Infrastructure.Options;

namespace Mokkit.Example1.Infrastructure.Logic.Events;

internal sealed class KafkaEventPublisher : IKafkaEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _kafkaOptions;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(
        IOptions<KafkaOptions> kafkaOptions,
        ILogger<KafkaEventPublisher> logger)
    {
        _kafkaOptions = kafkaOptions.Value;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            ClientId = _kafkaOptions.ClientId
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka producer error: {Error}", e.Reason))
            .Build();
    }

    public async Task PublishClientEventAsync(Guid clientId, string eventType, CancellationToken cancellationToken = default)
    {
        try
        {
            var topic = $"clients.{eventType}";
            var eventData = new
            {
                ClientId = clientId,
                EventType = eventType,
                Timestamp = DateTime.UtcNow
            };

            var message = new Message<string, string>
            {
                Key = clientId.ToString(),
                Value = JsonSerializer.Serialize(eventData),
                Headers = new Headers
                {
                    { "event-type", System.Text.Encoding.UTF8.GetBytes(eventType) },
                    { "timestamp", System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")) }
                }
            };

            _logger.LogInformation("Publishing client event: {EventType} for client {ClientId}", eventType, clientId);

            var deliveryResult = await _producer.ProduceAsync(topic, message, cancellationToken);
            
            _logger.LogInformation("Successfully published client event: {EventType} for client {ClientId} to partition {Partition} at offset {Offset}",
                eventType, clientId, deliveryResult.Partition, deliveryResult.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish client event: {EventType} for client {ClientId}", eventType, clientId);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}