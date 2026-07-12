---
title: Pick a mock library
description: Moq, NSubstitute, or FakeItEasy — choose the mock container that matches your library; the test shape never changes.
---

Mokkit doesn't ship a mocking framework — it *adapts* one. Three adapters come in the box, and swapping between
them changes only how you configure and read a double, never the shape of a test.

| Library | Package | Register with | Resolve in a verb as |
| --- | --- | --- | --- |
| **Moq** | `Mokkit.Containers.Moq` | `AddMock<T>(() => new Mock<T>())` | `Mock<T>` (inject `.Object`) |
| **NSubstitute** | `Mokkit.Containers.NSubstitute` | `AddSubstitute<T>()` | `T` directly |
| **FakeItEasy** | `Mokkit.Containers.FakeItEasy` | `AddFake<T>()` | `T` directly |

## Registering the doubles

Each adapter is a container builder you add to `TestStageSetup.Create(...)`, configured through `UseInit`:

```csharp
// Moq
var mocks = new MoqContainerBuilder().UseInit(m =>
{
    m.AddMock<IClientCacheService>(() => new Mock<IClientCacheService>());
    m.AddMock<IKafkaEventPublisher>(() => new Mock<IKafkaEventPublisher>());
    return Task.CompletedTask;
});

// NSubstitute
var subs = new NSubstituteContainerBuilder().UseInit(s =>
{
    s.AddSubstitute<IClientCacheService>();
    s.AddSubstitute<IKafkaEventPublisher>();
    return Task.CompletedTask;
});

// FakeItEasy
var fakes = new FakeItEasyContainerBuilder().UseInit(f =>
{
    f.AddFake<IClientCacheService>();
    f.AddFake<IKafkaEventPublisher>();
    return Task.CompletedTask;
});
```

## The one real difference: `Mock<T>` vs `T`

With **Moq**, a double is a `Mock<T>` wrapper — so your verbs resolve `Mock<T>` to configure it, and Mokkit
injects `mock.Object` into the real graph:

```csharp
public static ITestArrange Clock(this ITestArrange arrange, DateTime utcNow) =>
    arrange.Then(host => host.Execute<Mock<IDateTimeProvider>>(mock =>
        mock.SetupGet(x => x.UtcNow).Returns(utcNow)));
```

With **NSubstitute** and **FakeItEasy**, the double *is* the interface — no wrapper. Verbs resolve `T`
directly, and the same object is what the real service receives:

```csharp
public static ITestArrange HandlerSucceedsFor(this ITestArrange arrange, ...) =>
    arrange.Then(host => host.Execute<IRequestHandler<SaveClientCommand, SaveClientCommandResult>>(handler =>
        handler.Handle(Arg.Any<SaveClientCommand>(), Arg.Any<CancellationToken>())
            .Returns(new SaveClientCommandResult(true, id))));
```

Everything else — the [bridge into DI](/guides/real-di-container/), the test body, the Arrange/Act/Inspect
shape — is identical. The example proves it: its unit suite uses NSubstitute and its integration suite uses
Moq, over the very same Mokkit primitives.

:::note[Not just these three]
If your library or stack isn't covered, the adapter contract is tiny — see
[Write a custom container adapter](/guides/custom-container-adapter/). (The example's unit suite actually rolls
its own NSubstitute container as a demonstration.)
:::

## Next

- **[Wire a real DI container](/guides/real-di-container/)** — how the doubles reach the real service.
- **[Unit-test a service with a mock](/guides/unit-mocked-dependency/)** — a full worked test.
