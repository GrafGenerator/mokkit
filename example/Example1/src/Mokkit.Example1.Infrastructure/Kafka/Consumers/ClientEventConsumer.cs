using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mokkit.Example1.Application.Features.Client.SaveClient;
using Mokkit.Example1.Application.Logic.Messages;
using Mokkit.Example1.Common;
using Mokkit.Example1.Infrastructure.Options;

namespace Mokkit.Example1.Infrastructure.Kafka.Consumers;

internal sealed class ClientEventConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly KafkaOptions _kafkaOptions;
    private readonly ILogger<ClientEventConsumer> _logger;
    private readonly IConsumer<string, string> _consumer;

    public ClientEventConsumer(
        IServiceProvider serviceProvider,
        IOptions<KafkaOptions> kafkaOptions,
        ILogger<ClientEventConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _kafkaOptions = kafkaOptions.Value;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            GroupId = _kafkaOptions.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Error}", e.Reason))
            .Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _consumer.Subscribe(new[] { "clients.status-changed" });
            _logger.LogInformation("Kafka consumer started, subscribed to: clients.status-changed");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));
                    if (consumeResult?.Message != null)
                    {
                        await ProcessMessage(consumeResult, stoppingToken);
                        _consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message: {Error}", ex.Error.Reason);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Kafka consumer");
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
        }
    }

    private async Task ProcessMessage(ConsumeResult<string, string> consumeResult, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing message from topic {Topic}, partition {Partition}, offset {Offset}",
                consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);

            var message = JsonSerializer.Deserialize<ClientStatusChangedMessage>(consumeResult.Message.Value);
            if (message == null)
            {
                _logger.LogWarning("Failed to deserialize message: {MessageValue}", consumeResult.Message.Value);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var saveClientHandler = scope.ServiceProvider.GetRequiredService<IRequestHandler<SaveClientCommand, SaveClientCommandResult>>();
            var eventPublisher = scope.ServiceProvider.GetRequiredService<IKafkaEventPublisher>();

            var command = new SaveClientCommand
            {
                Operation = SaveOperationKind.Update,
                ClientData = new SaveClientData
                {
                    Id = message.ClientId,
                    Name = message.Name ?? string.Empty,
                    Email = message.Email ?? string.Empty,
                    Phone = message.Phone ?? string.Empty,
                    Status = message.Status
                }
            };

            var result = await saveClientHandler.Handle(command, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Successfully processed status change for client: {ClientId}", message.ClientId);
                
                // Send confirmation event
                await eventPublisher.PublishClientEventAsync(message.ClientId, "updated", cancellationToken);
            }
            else
            {
                _logger.LogError(result.Exception, "Failed to process status change for client: {ClientId}", message.ClientId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {MessageValue}", consumeResult.Message.Value);
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}

internal record ClientStatusChangedMessage
{
    public Guid ClientId { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public int Status { get; init; }
}
