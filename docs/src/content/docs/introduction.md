---
title: Introduction
description: What Mokkit is, and the one idea it's built around — tests that read like a story, as plain compilable C#.
---

Mokkit is a **test-orchestration toolkit for .NET**. It doesn't replace your test framework, your mocking
library, or your DI container — it sits on top of them and gives your tests a shape and a language.

That language is the whole point.

## The idea

Good tests describe a scenario. Read one aloud and it should sound like a sentence someone on the team
would say:

> Given a new client *Acme Corporation* that's already in the cache, when we create it, then the API returns
> **Created**, the row is in the database, and a `clients.created` event is published.

BDD tools like **Cucumber** and **SpecFlow** chase that readability with a separate language — Gherkin
feature files, plus "step bindings" that glue each English phrase to some C# behind the scenes. You get
prose, but you pay for it: a second syntax, a runtime binding layer, and steps that can drift out of sync
with the code without the compiler noticing.

Mokkit takes the other road. The same scenario, as a Mokkit test:

```csharp
await Arrange
    .NewClient(out var client, WithName("Acme Corporation"))
    .CacheHasClient(client);

var result = await Act(client);

await Inspect
    .WriteResult(result).Created()
    .Ensure(result, r => r.ClientId, out var id)
    .ApiClientMatches(id, name: "Acme Corporation")
    .DbClientExists(id)
    .EventPublished("clients.created", id);
```

It reads like the sentence above — but there is **no DSL**. `NewClient`, `CacheHasClient`, `Created`,
`ApiClientMatches`, `EventPublished` are just C# methods *you* wrote: your project's testing vocabulary.
Because it's plain code, you get everything the compiler and IDE give you — autocomplete, go-to-definition,
rename-refactoring, and the guarantee that a test that doesn't make sense **won't compile**.

## The shape: Arrange / Act / Inspect

Every Mokkit test follows the same three-phase story (Arrange-Act-Assert, with the assert phase named
*Inspect*):

- **Arrange** — set up the world and capture the artifacts later steps refer to.
- **Act** — perform the one thing under test; it yields a result.
- **Inspect** — observe the outcome. Inspect only *reads*; it never changes state.

Each phase is a fluent chain of the verbs you defined. See
[Arrange / Act / Inspect](/concepts/aai/) for the mechanics and
[Building your test vocabulary](/concepts/vocabulary/) for the part that matters most.

## Agnostic by design

Mokkit assumes nothing about the rest of your stack:

- **Test framework** — xUnit, NUnit, MSTest, TUnit; Mokkit is just calls inside your test methods.
- **Mocking** — first-class container packages for **Moq**, **NSubstitute** and **FakeItEasy** (or bring
  your own).
- **DI container** — **Microsoft.Extensions.DependencyInjection**, **Autofac**, **Castle Windsor**, or the
  dependency-free **Bag** container for tests that just need to hold a few instances.

The same vocabulary and the same test shape carry from a fast, fully-mocked **unit** test to an
**integration** test against a real database, up to a black-box **end-to-end** test that boots your whole
system in Docker. The [guides](/quickstart/) walk each of those.

## Next steps

- **[Why Mokkit?](/why-mokkit/)** — the honest comparison with BDD/DSL frameworks.
- **[Installation](/installation/)** — the packages and how to pick them.
- **[Quickstart](/quickstart/)** — a complete test in a few minutes.
