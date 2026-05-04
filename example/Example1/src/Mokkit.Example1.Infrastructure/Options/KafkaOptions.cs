namespace Mokkit.Example1.Infrastructure.Options;

public class KafkaOptions
{
    public const string SectionName = "Kafka";
    
    public string BootstrapServers { get; set; } = "localhost:9092";
    
    public string ClientId { get; set; } = "example1-service";
    
    public string ConsumerGroupId { get; set; } = "example1-consumers";
}
