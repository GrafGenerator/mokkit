namespace Mokkit.Example1.Infrastructure.Kafka.Consumers;

/// <summary>
/// Processes a single <c>clients.status-changed</c> message payload. Transport-free so it can be unit
/// tested without a Kafka broker.
/// </summary>
internal interface IClientStatusChangedProcessor
{
    /// <summary>
    /// Handles one raw message value: deserialize, apply the client update, and publish a confirmation.
    /// Never throws — failures are logged and swallowed so the consumer can commit and move on.
    /// </summary>
    Task ProcessAsync(string messageValue, CancellationToken cancellationToken = default);
}
