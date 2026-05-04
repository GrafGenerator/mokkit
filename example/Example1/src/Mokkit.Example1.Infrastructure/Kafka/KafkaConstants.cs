namespace Mokkit.Example1.Infrastructure.Kafka;

public static class KafkaConstants
{
    public const string ExternalBrokerName = "External";
    public const string InternalBrokerName = "Internal";

    public const string KafkaClientId = "client-management-queue-kafka";

    public const string ClientManagementTraceIdHeader = "traceId";
    public const string ClientManagementExternalProducerHeader = "externalProducer";
    public const string ClientManagementReplyTopicHeader = "replyTopic";
}