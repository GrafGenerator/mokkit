using Confluent.Kafka;
using Mokkit.Example1.Infrastructure.Kafka.Consumers;
using Mokkit.Inspect;
using Capture = Mokkit.Capture;

namespace Mokkit.Example1.Unit.Tests.Messaging.Consumer;

/// <summary>
/// Inspect building blocks for the consume step: assert the outcome and verify the substituted consumer +
/// processor interactions.
/// </summary>
internal static class InspectConsumer
{
    public static ITestInspect MessageConsumed(this ITestInspect inspect, bool processed)
    {
        return inspect.Then(_ => processed.ShouldBeTrue());
    }

    public static ITestInspect NoMessageConsumed(this ITestInspect inspect, bool processed)
    {
        return inspect.Then(_ => processed.ShouldBeFalse());
    }

    public static ITestInspect ForwardedToProcessor(this ITestInspect inspect, Capture<ConsumeResult<string, string>> message)
    {
        return inspect.Then(host =>
        {
            host.Execute<IClientStatusChangedProcessor>(processor =>
                processor.Received(1).ProcessAsync(message.Value!.Message.Value, Arg.Any<CancellationToken>()));
        });
    }

    public static ITestInspect OffsetCommitted(this ITestInspect inspect, Capture<ConsumeResult<string, string>> message)
    {
        return inspect.Then(host =>
        {
            host.Execute<IConsumer<string, string>>(consumer =>
                consumer.Received(1).Commit(message.Value!));
        });
    }

    public static ITestInspect NothingForwarded(this ITestInspect inspect)
    {
        return inspect.Then(host =>
        {
            host.Execute<IClientStatusChangedProcessor>(processor =>
                processor.DidNotReceive().ProcessAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()));
        });
    }

    public static ITestInspect NothingCommitted(this ITestInspect inspect)
    {
        return inspect.Then(host =>
        {
            host.Execute<IConsumer<string, string>>(consumer =>
                consumer.DidNotReceive().Commit(Arg.Any<ConsumeResult<string, string>>()));
        });
    }
}
