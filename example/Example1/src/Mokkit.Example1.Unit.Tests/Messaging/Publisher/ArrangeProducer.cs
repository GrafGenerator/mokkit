using Confluent.Kafka;
using Mokkit.Arrange;
using NSubstitute.ExceptionExtensions;

namespace Mokkit.Example1.Unit.Tests.Messaging.Publisher;

/// <summary>Arrange building blocks that shape the substituted Confluent <see cref="IProducer{TKey,TValue}"/>.</summary>
public static class ArrangeProducer
{
    public static ITestArrange ProducerAcceptsMessages(this ITestArrange arrange)
    {
        return arrange.Then(host =>
        {
            host.Execute<IProducer<string, string>>(producer =>
                producer.ProduceAsync(Arg.Any<string>(), Arg.Any<Message<string, string>>(), Arg.Any<CancellationToken>())
                    .Returns(new DeliveryResult<string, string>()));
        });
    }

    public static ITestArrange ProducerIsDown<TException>(this ITestArrange arrange)
        where TException : Exception, new()
    {
        return arrange.Then(host =>
        {
            host.Execute<IProducer<string, string>>(producer =>
                producer.ProduceAsync(Arg.Any<string>(), Arg.Any<Message<string, string>>(), Arg.Any<CancellationToken>())
                    .Throws(new TException()));
        });
    }
}
