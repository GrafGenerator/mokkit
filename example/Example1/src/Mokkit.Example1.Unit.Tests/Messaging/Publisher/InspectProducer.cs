using Confluent.Kafka;
using Mokkit.Inspect;

namespace Mokkit.Example1.Unit.Tests.Messaging.Publisher;

/// <summary>Inspect building blocks for the faked Confluent <see cref="IProducer{TKey,TValue}"/>.</summary>
public static class InspectProducer
{
    /// <summary>Verifies an event was produced to <c>clients.{eventType}</c> with the client key and payload.</summary>
    public static ITestInspect PublishedEvent(this ITestInspect inspect, Guid clientId, string eventType)
    {
        return inspect.Then(host =>
        {
            host.Execute<IProducer<string, string>>(producer =>
                producer.Received(1).ProduceAsync(
                    $"clients.{eventType}",
                    Arg.Is<Message<string, string>>(m =>
                        m.Key == clientId.ToString() && m.Value.Contains(clientId.ToString())),
                    Arg.Any<CancellationToken>()));
        });
    }

    /// <summary>Verifies the act captured a fault of the expected type (e.g. the publisher rethrew).</summary>
    public static ITestInspect Faulted<TException>(this ITestInspect inspect, Exception? captured)
        where TException : Exception
    {
        return inspect.Then(_ => captured.ShouldBeOfType<TException>());
    }
}
