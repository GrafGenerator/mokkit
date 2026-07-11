# Mokkit

[![NuGet](https://img.shields.io/nuget/vpre/Mokkit.svg)](https://www.nuget.org/packages/Mokkit)
[![CI](https://github.com/GrafGenerator/mokkit/actions/workflows/ci.yml/badge.svg)](https://github.com/GrafGenerator/mokkit/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Write tests that read like a story in your domain's language — as ordinary, compilable C#.**

Mokkit gives your tests the readability of BDD tools like Cucumber/SpecFlow, but with **no DSL**: no feature
files, no step bindings, no runtime glue. The "steps" are just C# extension methods you author — your
project's testing vocabulary — so you keep full IDE support (autocomplete, go-to-definition, refactoring) and
a test that doesn't make sense simply won't compile.

It's a thin orchestration layer, not a framework: run it inside xUnit / NUnit / MSTest, mock with
Moq / NSubstitute / FakeItEasy, and wire with Microsoft DI / Autofac / Castle Windsor (or the dependency-free
Bag). The same Arrange / Act / Inspect vocabulary scales from a mocked unit test to a full Testcontainers
end-to-end run.

```csharp
// A Mokkit test reads like the scenario it describes.
[Fact]
public async Task Suspending_a_client_reflects_everywhere()
{
    await Arrange
        .NewClient(out var clientId, WithName("Acme Corporation"))
        .CacheHasClient(clientId);

    await Act(clientId, ClientStatus.Suspended);

    await Inspect
        .ApiClientEventually(clientId, c => c.Status == Suspended)
        .DbClient(clientId, c => c!.Status.ShouldBe(Suspended))
        .EventPublished("clients.updated", clientId);
}
```

`NewClient`, `CacheHasClient`, `ApiClientEventually`, `DbClient`, `EventPublished` aren't Mokkit APIs — they're
your verbs. Mokkit provides the Arrange / Act / Inspect shape and the machinery underneath.

## Install

Core + a DI adapter + a mock adapter (prerelease for now):

```bash
dotnet add package Mokkit --prerelease
dotnet add package Mokkit.Containers.Microsoft.Extensions.DependencyInjection --prerelease
dotnet add package Mokkit.Containers.NSubstitute --prerelease
```

## Packages

| Package | Purpose |
| --- | --- |
| `Mokkit` | Core: Stage, Arrange/Act/Inspect, captures, the `[MokkitCapture]` source generator |
| `Mokkit.Containers.Microsoft.Extensions.DependencyInjection` · `.Autofac` · `.CastleWindsor` | DI container adapters |
| `Mokkit.Containers.Moq` · `.NSubstitute` · `.FakeItEasy` | Mock library adapters |
| `Mokkit.Containers.Bag` | Dependency-free "hold a few instances" container |

## Documentation

Full guides, concepts and API reference: **[mokkit.net](https://mokkit.net)**

- [Introduction](https://mokkit.net/introduction/) · [Why Mokkit? (vs BDD/DSL)](https://mokkit.net/why-mokkit/) · [Quickstart](https://mokkit.net/quickstart/)
- [Building your test vocabulary](https://mokkit.net/concepts/vocabulary/) — the idea Mokkit is built around

A worked, three-tier example (unit / integration / e2e) lives in [`example/Example1`](example/Example1).

## License

[MIT](LICENSE) © Nikita Ivanov
