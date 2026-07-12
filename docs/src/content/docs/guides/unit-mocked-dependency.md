---
title: Unit-test a service with a mocked dependency
description: The bread-and-butter Mokkit test — a real service, its collaborators substituted, driven and verified through your own vocabulary.
---

The most common test: one real service, its dependencies replaced with test doubles you drive and verify. This
guide builds one end to end using **NSubstitute**, lifted from the example's unit suite.

The system under test is a `ClientStatusChangedProcessor` that maps an incoming message to a save command,
dispatches it through a handler, and publishes a confirmation on success. Both collaborators — the handler and
the publisher — are substituted.

## 1. A fixture: the real SUT + substituted dependencies

A fixture declares one composition: the real service, plus substitutes for *its* direct dependencies. (See
[the Stage](/concepts/stage/) for why it's one fixture per system-under-test.)

```csharp
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
```

The `BaseStageFixture` behind this composes a substitute container with a real Microsoft DI container and
**bridges** every substitute into DI, so the real processor resolves the *same* doubles the test arranges. That
bridge is the subject of its own guide — [Wire a real DI container](/guides/real-di-container/); here we just
use it.

## 2. Vocabulary: arrange the doubles, inspect the calls

**Arrange verbs** build the incoming message and configure the substituted handler:

```csharp
public static ITestArrange HandlerSucceedsFor(
    this ITestArrange arrange, Capture<ClientStatusChangedMessage> message) =>
    arrange.Then(host => host.Execute<IRequestHandler<SaveClientCommand, SaveClientCommandResult>>(handler =>
        handler.Handle(Arg.Any<SaveClientCommand>(), Arg.Any<CancellationToken>())
            .Returns(new SaveClientCommandResult(true, message.Value!.ClientId))));
```

**Inspect verbs** verify how the SUT drove those doubles — they only *read*:

```csharp
public static ITestInspect HandledUpdate(
    this ITestInspect inspect, Capture<ClientStatusChangedMessage> message) =>
    inspect.Then(host => host.Execute<IRequestHandler<SaveClientCommand, SaveClientCommandResult>>(handler =>
        handler.Received(1).Handle(
            Arg.Is<SaveClientCommand>(c =>
                c.Operation == SaveOperationKind.Update &&
                c.ClientData.Id == message.Value!.ClientId),
            Arg.Any<CancellationToken>())));

public static ITestInspect ConfirmationPublishedFor(
    this ITestInspect inspect, Capture<ClientStatusChangedMessage> message) =>
    inspect.Then(host => host.Execute<IKafkaEventPublisher>(publisher =>
        publisher.Received(1).PublishClientEventAsync(message.Value!.ClientId, "updated", Arg.Any<CancellationToken>())));
```

`host.Execute<T>(...)` resolves `T` from the stage. When `T` is a substituted type, you get the substitute — the
very one the real processor will also resolve. See [Building your test vocabulary](/concepts/vocabulary/) for
the verb-authoring patterns.

## 3. The test

With the vocabulary in place, the test is three lines of your own language:

```csharp
public sealed class ClientStatusChangedProcessorTests : BaseUnitTest<ProcessorFixture>
{
    public ClientStatusChangedProcessorTests(ProcessorFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ValidMessage_UpdatesClient_AndPublishesConfirmation()
    {
        // ARRANGE — an incoming message, and a handler set to succeed for it.
        await Arrange
            .IncomingStatusChange(out var message, status: (int)ClientStatus.Suspended)
            .HandlerSucceedsFor(message);

        // ACT — run the real processor over the message.
        await Stage.Act().Then(host =>
            host.ExecuteAsync<IClientStatusChangedProcessor>(p => p.ProcessAsync(KafkaMessageFaker.ToJson(message.Value!))));

        // INSPECT — it dispatched an Update and published the confirmation.
        await Inspect
            .HandledUpdate(message)
            .ConfirmationPublishedFor(message);
    }
}
```

The negative cases read just as clearly — swap `HandlerSucceedsFor` → `HandlerFailsFor` and assert
`.NoConfirmationPublished()`, or feed `Process("{ not-valid-json")` and assert `.NotHandled()`.

:::note[Which mock library?]
This suite uses NSubstitute (`AddSubstitute`, `Received`, `Arg`). Mokkit adapts Moq and FakeItEasy just as
well — only the double's API changes, never the test shape. See
[Pick a mock library](/guides/mock-libraries/).
:::

## Next

- **[Wire a real DI container](/guides/real-di-container/)** — how the substitutes reach the real service.
- **[Deterministic time & ids](/guides/deterministic-time-ids/)** — substitute the clock and id generator.
- **[Integration-test against a real database](/guides/integration-database/)** — the same shape, real infra.
