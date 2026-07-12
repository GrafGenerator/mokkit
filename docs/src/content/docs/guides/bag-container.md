---
title: The Bag container
description: A dependency-free container that just holds the instances you give it — perfect for a first test or a stage of pre-built external clients.
---

Not every test needs a DI framework. The **Bag** (`Mokkit.Containers.Bag`) is a trivial container that holds
instances you hand it — no auto-wiring, no options, no dependency on Microsoft DI. It's the right tool for two
situations: your very first test, and a stage that just needs to hold some pre-built clients.

## Two ways to register

```csharp
var setup = await TestStageSetup.Create(
    new BagContainerBuilder()
        .AddInstance(email)                       // a singleton you own; reused across resolves
        .AddInstance(new SignupService(email))
        .AddFactory(() => new DbContext(options))); // built fresh per stage, disposed with the stage

var stage = setup.EnterStage();
```

- **`AddInstance(x)`** — stores the object as-is. The *same* instance is returned on every `Execute<T>`, and
  Mokkit does **not** dispose it (you own its lifetime). Ideal for a shared mock or HTTP client.
- **`AddFactory(() => ...)`** — one instance per stage, created on first resolve and **disposed when the stage
  is disposed**. Ideal for a per-test `DbContext`.

Resolving is the same `Execute`/`ExecuteAsync` as any container:

```csharp
stage.Execute<IEmailSender>(email => email.Received(1).SendWelcome(address));
```

## When it shines

**A first test.** Hand-wire a substitute and the service under test — the same substitute goes into both, so you
can drive it and verify it, with zero container ceremony (this is the [quickstart](/quickstart/) setup).

**A stage of external clients.** In the [E2E suite](/guides/end-to-end/), the whole system runs in Docker and the
stage only needs to *hold clients pointed at it* — no mocks, no DI graph. Bag is a perfect fit:

```csharp
var external = new BagContainerBuilder().UseInit(bag =>
{
    bag.AddInstance(new HttpClient { BaseAddress = apiBaseAddress });
    bag.AddInstance<IProducer<string, string>>(BuildKafkaProducer(bootstrap));
    bag.AddInstance(new KafkaProbe(bootstrap));
    bag.AddFactory(() => new ExampleContext(NpgsqlOptions(connectionString)));  // fresh + disposed per stage
    return Task.CompletedTask;
});
```

## When to reach for more

The moment you want to test a *real* object graph — a service whose dependencies are themselves resolved and
constructed by a container — move up to a real DI adapter and bridge your mocks in. That's the
[real DI container](/guides/real-di-container/) guide.

## Next

- **[Quickstart](/quickstart/)** — a first test built on the Bag.
- **[Full black-box E2E](/guides/end-to-end/)** — the Bag holding external clients.
- **[Wire a real DI container](/guides/real-di-container/)** — when hand-wiring isn't enough.
