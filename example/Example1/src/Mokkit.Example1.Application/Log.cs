using Microsoft.Extensions.Logging;

namespace Mokkit.Example1.Application;

public static partial class Log
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Started client operation: {Operation} for ClientId: {ClientId}")]
    public static partial void ClientOperationBegin(this ILogger logger, string operation, Guid? clientId);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Completed client operation: {Operation} for ClientId: {ClientId}, Success: {Success}")]
    public static partial void ClientOperationEnd(this ILogger logger, string operation, Guid? clientId, bool success);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Error during client operation: {Operation} for ClientId: {ClientId}")]
    public static partial void ClientOperationError(this ILogger logger, Exception ex, string operation, Guid? clientId);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Client cached: {ClientId}")]
    public static partial void ClientCached(this ILogger logger, Guid clientId);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Client found in cache: {ClientId}")]
    public static partial void ClientFoundInCache(this ILogger logger, Guid clientId);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Warning,
        Message = "Client not found in cache: {ClientId}")]
    public static partial void ClientNotFoundInCache(this ILogger logger, Guid clientId);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "Publishing Kafka event: {EventType} for ClientId: {ClientId}")]
    public static partial void PublishingKafkaEvent(this ILogger logger, string eventType, Guid clientId);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Information,
        Message = "Kafka event published successfully: {EventType} for ClientId: {ClientId}")]
    public static partial void KafkaEventPublished(this ILogger logger, string eventType, Guid clientId);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Error,
        Message = "Failed to publish Kafka event: {EventType} for ClientId: {ClientId}")]
    public static partial void KafkaEventPublishFailed(this ILogger logger, Exception ex, string eventType, Guid clientId);
}
