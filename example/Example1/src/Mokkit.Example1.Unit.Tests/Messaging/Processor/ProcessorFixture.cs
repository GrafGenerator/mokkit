using Microsoft.Extensions.DependencyInjection;
using Mokkit.Example1.Application.Features.Client.SaveClient;
using Mokkit.Example1.Application.Logic.Messages;
using Mokkit.Example1.Common;
using Mokkit.Example1.Infrastructure.Kafka.Consumers;
using Mokkit.Example1.Unit.Tests.Containers;

namespace Mokkit.Example1.Unit.Tests.Messaging.Processor;

/// <summary>
/// SUT: the real <c>ClientStatusChangedProcessor</c>; dependencies: substituted save handler + publisher,
/// which the processor resolves from a real DI scope (Mokkit serves them in via <c>ResolveFromStage</c>).
/// </summary>
public sealed class ProcessorFixture : BaseStageFixture
{
    protected override void ConfigureSubstitutes(ISubstituteCollection substitutes)
    {
        substitutes.AddSubstitute<IRequestHandler<SaveClientCommand, SaveClientCommandResult>>();
        substitutes.AddSubstitute<IKafkaEventPublisher>();
    }

    protected override void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<IClientStatusChangedProcessor, ClientStatusChangedProcessor>();
}
