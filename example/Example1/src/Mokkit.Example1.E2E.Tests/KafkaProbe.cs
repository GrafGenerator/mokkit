using Confluent.Kafka;

namespace Mokkit.Example1.E2E.Tests;

/// <summary>
/// A host-side Kafka consumer used by inspects to confirm the service published an event.
/// Reads a topic from the beginning with a throwaway group and looks for a message by key.
/// </summary>
public sealed class KafkaProbe
{
    private readonly string _bootstrapServers;

    public KafkaProbe(string bootstrapServers) => _bootstrapServers = bootstrapServers;

    public Task<bool> SawMessageKeyed(string topic, string key, TimeSpan timeout) => Task.Run(() =>
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = $"e2e-probe-{Guid.NewGuid():N}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topic);

        try
        {
            var deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
                if (result?.Message?.Key == key)
                {
                    return true;
                }
            }

            return false;
        }
        finally
        {
            consumer.Close();
        }
    });
}
