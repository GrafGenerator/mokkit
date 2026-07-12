---
title: Quickstart
description: Write a complete Mokkit test — build a Stage, define a verb, and inspect an outcome — in a few minutes.
---

This walks through a full (if tiny) Mokkit test with xUnit and NSubstitute. See
[Installation](/installation/) for the packages.

## 1. The system under test

A service with one dependency we'll want to substitute:

```csharp
public interface IEmailSender
{
    Task SendWelcome(string address);
}

public sealed class SignupService(IEmailSender email)
{
    public async Task<Guid> Register(string address)
    {
        await email.SendWelcome(address);
        return Guid.NewGuid();
    }
}
```

## 2. Build a Stage

The **Stage** is where your services live during a test. Here we use the dependency-free **Bag** container
to hold a substitute and the service under test — the same substitute instance goes into both, so we can
drive it *and* verify it:

```csharp
using Mokkit.Containers.Bag;
using Mokkit.Suite;
using NSubstitute;

public sealed class SignupTests
{
    private static async Task<TestStage> NewStage()
    {
        var email = Substitute.For<IEmailSender>();

        var setup = await TestStageSetup.Create(
            new BagContainerBuilder()
                .AddInstance(email)
                .AddInstance(new SignupService(email)));

        return setup.EnterStage();
    }
}
```

:::tip[The real-world setup]
Hand-wiring with Bag is perfect for a first test. In practice you'll usually resolve the *real* service from
your DI container and let a mock container bridge substitutes into it — see
[Wire a real DI container](/concepts/containers/). The test body below doesn't change.
:::

## 3. Define a verb (your vocabulary)

An **Inspect verb** is a C# extension method that observes an outcome. This one reads "a welcome email was
sent to…":

```csharp
using Mokkit.Inspect;

public static class SignupVocabulary
{
    public static ITestInspect WelcomeEmailSent(this ITestInspect inspect, string toAddress) =>
        inspect.Then(host => host.Execute<IEmailSender>(email =>
            email.Received(1).SendWelcome(toAddress)));
}
```

## 4. Write the test

Now the test reads as Arrange → Act → Inspect. (This one needs no arrange.)

```csharp
[Fact]
public async Task Registering_a_user_sends_a_welcome_email()
{
    var stage = await NewStage();

    // ACT — the Act phase resolves the service, runs the one thing under test, and returns its result.
    var id = await stage.Act().Returning(host =>
        host.ExecuteAsync<SignupService, Guid>(service => service.Register("acme@example.com")));

    // INSPECT — observe the outcome through your vocabulary.
    await stage.Inspect()
        .WelcomeEmailSent("acme@example.com");

    Assert.NotEqual(Guid.Empty, id);
}
```

:::note[Act is a phase too]
`stage.Act()` starts the Act phase, symmetric with `Arrange` and `Inspect`. `.Returning(...)` is the flavor
that hands a result back; there are also void and capture flavors. In a real project you'd wrap this in an
**Act verb** — `await Act.RegisterUser("acme@example.com")` — exactly like the Inspect verb above. See
[Arrange / Act / Inspect](/concepts/aai/#act).
:::

## What just happened

- `TestStageSetup.Create(...)` composed your containers; `EnterStage()` gave you a fresh **Stage** for the
  test.
- `stage.Act().Returning(...)` started the **Act** phase, **resolved** the service from the stage, ran it, and
  returned its result.
- `stage.Inspect().WelcomeEmailSent(...)` ran your **Inspect** verb, which resolved the same `IEmailSender`
  from the stage and verified the call.

The verb `WelcomeEmailSent` is the seed of your project's **vocabulary**. As you add
`Arrange` verbs (to set up state) and more `Inspect` verbs (to observe it), tests become short, readable
compositions of sentences — with full IDE and compile-time support.

## Next

- **[Arrange / Act / Inspect](/concepts/aai/)** — the mechanics of each phase.
- **[Building your test vocabulary](/concepts/vocabulary/)** — the idea Mokkit is built around.
