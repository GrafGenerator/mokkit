---
title: Why Mokkit?
description: How Mokkit compares to BDD/DSL frameworks like Cucumber and SpecFlow — readable tests, without the DSL tax.
---

Mokkit exists for one reason: to get the **readability** of BDD without the **cost** of a DSL.

## The BDD promise, and its price

Tools like Cucumber and SpecFlow let you write scenarios in near-English Gherkin:

```gherkin
Scenario: Create a client
  Given a new client "Acme Corporation"
  When I create it
  Then the API returns Created
  And a "clients.created" event is published
```

That's genuinely readable. But the prose isn't the program — it's a script that has to be *bound* to code:

```csharp
[Given(@"a new client ""(.*)""")]
public void GivenANewClient(string name) { /* ... */ }
```

So you carry a second language and a runtime binding layer, and you inherit their problems:

- **Two artifacts to keep in sync.** A `.feature` file and its step definitions live apart. Rename a step's
  wording and the binding silently breaks — at *runtime*, not compile time.
- **No compiler, weak IDE.** Steps are matched by regex/string at run time. Go-to-definition, rename, and
  "find all usages" are best-effort at most; a typo in a step is a runtime failure.
- **Indirection for its own sake.** Every phrase bounces through a binding before reaching the code you
  actually care about.
- **Parameters through strings.** Values arrive as text and get parsed back into types.

## Mokkit's answer: it's just code

Mokkit keeps the readable, sentence-like scenario but drops the separate language. The "steps" are C#
extension methods you author — your **domain vocabulary** — and a test simply composes them:

```csharp
await Arrange.NewClient(out var client, WithName("Acme Corporation"));
var result = await Act(client);
await Inspect.WriteResult(result).Created().EventPublished("clients.created", result);
```

Because a Mokkit test is ordinary code, the trade-offs invert:

| | BDD / DSL (Cucumber, SpecFlow) | Mokkit |
| --- | --- | --- |
| Readability | ✅ Gherkin prose | ✅ Sentence-like C# |
| Separate language to learn | Yes (Gherkin + bindings) | **No** |
| Steps bound at | Runtime (regex/strings) | **Compile time** |
| IDE navigation / rename / find-usages | Limited | **Full** |
| A nonsensical/typo'd step | Fails at runtime | **Fails to compile** |
| Parameters | Strings, re-parsed | **Typed** |
| Debug into a step | Through the binding layer | **Straight into the method** |

The headline: with Mokkit, **a test that doesn't make sense won't compile**, and the same `dotnet build`
that checks your product code checks your tests' vocabulary.

## When BDD is still the better fit

Mokkit is not anti-BDD; it's a different trade-off. Reach for a Gherkin tool when:

- **Non-developers author or read the scenarios.** Business analysts editing `.feature` files is a workflow
  Mokkit doesn't try to replace — Mokkit's audience is engineers who live in an IDE.
- **The living documentation *is* the deliverable**, and plain-English feature files are a contractual
  artifact.

If your scenarios are written and maintained by the same engineers who write the code, Mokkit gives you the
readability with none of the DSL tax.

## What Mokkit is *not*

- Not a test framework — it runs inside xUnit / NUnit / MSTest.
- Not a mocking library — it wraps Moq / NSubstitute / FakeItEasy.
- Not a DI container — it wraps yours (or ships a trivial one).

It's the thin layer that gives those tools a shared shape and a readable language. Next:
**[Installation](/installation/)**.
