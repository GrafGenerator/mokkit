using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mokkit.Example1.Application.Features.Client.SaveClient;
using Mokkit.Example1.Application.Logic.Messages;
using Mokkit.Example1.Common;

namespace Mokkit.Example1.Infrastructure.Kafka.Consumers;

/// <summary>
/// Transport-free processing of <c>clients.status-changed</c> messages: deserialize → resolve the save
/// handler + publisher from a fresh DI scope → map to an Update command → handle → publish a
/// confirmation on success. Extracted from <see cref="ClientEventConsumer"/> so it is unit testable.
/// </summary>
internal sealed class ClientStatusChangedProcessor : IClientStatusChangedProcessor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ClientStatusChangedProcessor> _logger;

    public ClientStatusChangedProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<ClientStatusChangedProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ProcessAsync(string messageValue, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = JsonSerializer.Deserialize<ClientStatusChangedMessage>(messageValue);
            if (message == null)
            {
                _logger.LogWarning("Failed to deserialize message: {MessageValue}", messageValue);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var saveClientHandler = scope.ServiceProvider
                .GetRequiredService<IRequestHandler<SaveClientCommand, SaveClientCommandResult>>();
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
            _logger.LogError(ex, "Error processing message: {MessageValue}", messageValue);
        }
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
