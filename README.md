<p align="center">
  <img src="https://raw.githubusercontent.com/GrafGenerator/Mokkit/main/assets/banner.png" alt="Mokkit — write tests that read like a story, in plain C#" width="820">
</p>

[![NuGet](https://img.shields.io/nuget/v/Mokkit.svg)](https://www.nuget.org/packages/Mokkit)
[![CI](https://github.com/GrafGenerator/Mokkit/actions/workflows/ci.yml/badge.svg)](https://github.com/GrafGenerator/Mokkit/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Mokkit gives your tests the readability of BDD tools like Cucumber/SpecFlow, but with **no DSL**: no feature
files, no step bindings, no runtime glue. The "steps" are just C# extension methods you author — your
project's testing vocabulary — so you keep full IDE support (autocomplete, go-to-definition, refactoring) and
a test that doesn't make sense simply won't compile.

It's a thin orchestration layer, not a framework: run it inside xUnit / NUnit / MSTest / TUnit, mock with
Moq / NSubstitute / FakeItEasy, and wire with Microsoft DI / Autofac / Castle Windsor (or the dependency-free
Bag). The same Arrange / Act / Inspect vocabulary scales from a mocked unit test to a full Testcontainers
end-to-end run.

```csharp
// A Mokkit test reads like the scenario it describes.
[Fact]
public async Task CalculateDiscount_ForVipUser_AppliesTieredRate()
{
    await Arrange
        .UserExists(out var user, WithStatus(UserStatus.Vip))   // build the user, set up its repository
        .DiscountRateIs(UserStatus.Vip, rate: 0.15m);           // set up the rates repository

    var result = await Act.CalculateDiscount(user, orderTotal: 100m);

    await Inspect
        .OkResult(result).DiscountAppliedFor(user, expectedAmount: 15m)
        .Ensure(result, r => r.UserId, out var userId)          // guard the id, then reuse it
        .ThenAll(                                               // these three run in parallel
            b => b.UserRepositoryQueried(userId),
            b => b.UserCalculationRepositoryQueried(userId),
            b => b.RateRepositoryQueried(UserStatus.Vip));
}
```

`UserExists`, `DiscountRateIs`, `UserRepositoryQueried` aren't Mokkit APIs — they're **your verbs**. Mokkit
provides the Arrange / Act / Inspect shape and the machinery underneath.

## What a verb costs you

That's the fair question, so here's the whole answer. A verb is an extension method that does the coupled
setup (or the coupled verification) once, and hides it behind a name from your domain:

```csharp
public static class ArrangeDiscount
{
    public static ITestArrange UserExists(
        this ITestArrange arrange,
        out Capture<User> userCapture,
        Action<User>? mutate = null)
    {
        var capture = Capture.Start(out userCapture);

        return arrange.Then(host =>
        {
            var user = new User { Id = Guid.NewGuid() };
            mutate?.Invoke(user);

            host.Execute<IUserRepository>(repo =>
                repo.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user));

            capture.Set(user);
        });
    }
}

public static class InspectDiscount
{
    public static ITestInspect UserRepositoryQueried(this ITestInspect inspect, Guid userId)
    {
        return inspect.Then(host =>
            host.Execute<IUserRepository>(repo =>
                repo.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>())));
    }
}
```

Written once per fixture and living next to the tests. They start paying off quickly — most get reused across
every test that touches the same area, and a good half end up reused far beyond it.

Your mocking and assertion libraries stay yours: the snippets above use NSubstitute, but Moq, FakeItEasy,
Shouldly, FluentAssertions or plain `Assert` all work the same way.

## The same vocabulary, from unit to end-to-end

Because the verbs are just extension methods over a context Mokkit hands them, the *same* test can run against
mocks or against real infrastructure — you swap the helper implementations, not the test:

```csharp
[Fact]
public async Task CalculateDiscount_ForVipUser_AppliesTieredRate()
{
    await Arrange
        .UserExists(out var user, WithStatus(UserStatus.Vip))   // ← now creates the user via the API
        .DiscountRateIs(UserStatus.Vip, 0.15m);                 // ← now sets the rate via the API

    var result = await Act.CalculateDiscount(user, orderTotal: 100m);   // ← now calls the real endpoint

    await Inspect
        .OkResult(result).DiscountAppliedFor(user, expectedAmount: 15m)
        .Ensure(result, r => r.UserId, out var userId)
        .Ensure(result, r => r.CalculationId, out var calculationId)
        .UserCalculationStored(userId, calculationId, 15m)      // e2e-only: assert against the database
        .DiscountEventPublishedFor(user, 15m);                  // e2e-only: assert against the broker
}
```

The Arrange and Act lines are identical; end-to-end just adds the inspects that only make sense against real
infrastructure.

A worked three-tier example — the same feature covered by unit, integration (Testcontainers + Postgres) and
end-to-end (API + Kafka) suites, across xUnit, NUnit and TUnit — lives in
**[`example/Example1`](example/Example1)**.

## Install

Core + a DI adapter + a mock adapter:

```bash
dotnet add package Mokkit
dotnet add package Mokkit.Containers.Microsoft.Extensions.DependencyInjection
dotnet add package Mokkit.Containers.NSubstitute
```

## Packages

| Package | Purpose |
| --- | --- |
| `Mokkit` | Core: Stage, Arrange/Act/Inspect, captures, the `[MokkitCapture]` source generator |
| `Mokkit.Containers.Microsoft.Extensions.DependencyInjection` · `.Autofac` · `.CastleWindsor` | DI container adapters |
| `Mokkit.Containers.Moq` · `.NSubstitute` · `.FakeItEasy` | Mock library adapters |
| `Mokkit.Containers.Bag` | Dependency-free "hold a few instances" container |

No package is needed for your test framework — Mokkit runs inside xUnit, NUnit, MSTest or TUnit as-is.

## Status

**v0.4.0**, MIT, targets .NET Standard 2.0. It has been in daily use on a production codebase for two months,
and it is actively developed: expect API changes before 1.0, with verbose output and test-report integration
among the things still missing. Adapters for other containers and mock libraries are straightforward to add.

What it most needs right now is feedback that isn't mine. Would you write your tests this way — and if not,
what puts you off? [Discussions](https://github.com/GrafGenerator/Mokkit/discussions) is the place for that,
and for anything open-ended; a reproducible bug or a concrete feature request is best as an
[issue](https://github.com/GrafGenerator/Mokkit/issues).

## Documentation

Full guides, concepts and API reference: **[mokkit.net](https://mokkit.net)**

- [Introduction](https://mokkit.net/introduction/) · [Why Mokkit? (vs BDD/DSL)](https://mokkit.net/why-mokkit/) · [Quickstart](https://mokkit.net/quickstart/)
- [Building your test vocabulary](https://mokkit.net/concepts/vocabulary/) — the idea Mokkit is built around

## License

[MIT](LICENSE) © Nikita Ivanov
