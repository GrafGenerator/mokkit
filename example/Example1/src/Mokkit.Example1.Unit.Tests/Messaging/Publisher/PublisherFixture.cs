using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Mokkit.Example1.Unit.Tests.Containers;

namespace Mokkit.Example1.Unit.Tests.Messaging.Publisher;

/// <summary>
/// Dependency: a substituted Confluent <see cref="IProducer{TKey,TValue}"/>. The SUT
/// (<c>KafkaEventPublisher</c>) is constructed inside the test's Act, pulling this producer from the stage.
/// </summary>
public sealed class PublisherFixture : BaseStageFixture
{
    protected override void ConfigureSubstitutes(ISubstituteCollection substitutes) =>
        substitutes.AddSubstitute<IProducer<string, string>>();

    protected override void ConfigureServices(IServiceCollection services)
    {
        // SUT is newed up in Act with its dependencies resolved from the stage.
    }
}
