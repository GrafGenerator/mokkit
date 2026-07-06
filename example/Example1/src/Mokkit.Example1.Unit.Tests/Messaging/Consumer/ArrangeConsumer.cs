using Confluent.Kafka;
using Mokkit.Arrange;
using Capture = Mokkit.Capture;

namespace Mokkit.Example1.Unit.Tests.Messaging.Consumer;

/// <summary>Arrange building blocks that shape the substituted Confluent <see cref="IConsumer{TKey,TValue}"/>.</summary>
public static class ArrangeConsumer
{
    /// <summary>A message is waiting on the topic. The crafted <see cref="ConsumeResult{TKey,TValue}"/> is captured.</summary>
    public static ITestArrange MessageAvailable(
        this ITestArrange arrange,
        out Capture<ConsumeResult<string, string>> messageCapture,
        string? value = null)
    {
        var capture = Capture.Start(out messageCapture);
        return arrange.Then(host =>
        {
            var result = new ConsumeResult<string, string>
            {
                Topic = "clients.status-changed",
                Message = new Message<string, string>
                {
                    Key = "client-key",
                    Value = value ?? $"{{\"clientId\":\"{Guid.NewGuid()}\"}}"
                }
            };

            host.Execute<IConsumer<string, string>>(consumer =>
                consumer.Consume(Arg.Any<TimeSpan>()).Returns(result));

            capture.Set(result);
        });
    }

    /// <summary>No message is available within the poll window.</summary>
    public static ITestArrange NoMessageAvailable(this ITestArrange arrange)
    {
        return arrange.Then(host =>
        {
            host.Execute<IConsumer<string, string>>(consumer =>
                consumer.Consume(Arg.Any<TimeSpan>()).Returns((ConsumeResult<string, string>?)null));
        });
    }
}
