namespace Mokkit.Example1.Application.Logic.Messages;

public interface IKafkaEventPublisher
{
    Task PublishClientEventAsync(Guid clientId, string eventType, CancellationToken cancellationToken = default);
}