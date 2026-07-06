using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Mokkit.Example1.Infrastructure.Kafka.Consumers;
using Mokkit.Example1.Unit.Tests.Containers;

namespace Mokkit.Example1.Unit.Tests.Messaging.Consumer;

/// <summary>
/// Dependencies: a substituted Confluent <see cref="IConsumer{TKey,TValue}"/> and a substituted
/// <c>IClientStatusChangedProcessor</c>. The SUT (<c>ClientEventConsumer</c>) is constructed inside the
/// test's Act, pulling both from the stage.
/// </summary>
public sealed class ConsumerFixture : BaseStageFixture
{
    protected override void ConfigureSubstitutes(ISubstituteCollection substitutes)
    {
        substitutes.AddSubstitute<IConsumer<string, string>>();
        substitutes.AddSubstitute<IClientStatusChangedProcessor>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        // SUT is newed up in Act with its dependencies resolved from the stage.
    }
}
