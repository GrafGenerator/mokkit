---
title: Conventions cheat-sheet
description: The one-screen summary of Mokkit conventions — pin it up while writing tests.
---

The short version of the [project structure](/reference/project-structure/) and
[vocabulary](/concepts/vocabulary/) guides. Distilled from the example's three convention docs.

## The cardinal rule

> **Arrange and Act *produce* artifacts. Inspect only *observes*** — reads OK, mutations not.

- If an **inspect** performs the thing under test → lift it into **Act** and pass the artifact on.
- If an **act** asserts → move the assertion into **Inspect**; Act should just return/capture the artifact.

## Test bodies

- **No raw setup or assertions in a test body.** Every value is a business-named `Arrange`; every check a
  business-named `Inspect`.
- **Three blocks, in order:** `await Arrange…` → `await Act…` → `await Inspect…`, nothing else interleaved.
- The **what** lives in the test body; the **how** lives in reusable verbs.

## Vocabulary

- Verbs are **extension methods** on `ITestArrange` / `ITestAct` / `ITestInspect`, returning the same, ending
  in `.Then(host => …)`.
- **Colocate** per feature/SUT: `Arrange<Feature>.cs` · `Act<Feature>.cs` · `Inspect<Feature>.cs` next to the
  tests, same namespace.
- **Cross-feature** verbs live in a **root-namespace** `Helpers/` folder (in scope everywhere).
- **Defaults at the top, variation via mutators** — build from one default + `params …Fn[]`; tests list only
  what differs (`WithName`, `WithEmail`, …).
- Set up mocks inside `host.Execute<Mock<T>>(…)`; seed the DB inside `host.ExecuteAsync<Context>(…)`. **Await**
  async work — return the `Task` from `.Then(async host => await …)`.

## Captures

- **Capture, don't return.** Surface values built in Arrange via an `out Capture<T>` / `out Trapture<T>`.
- **Captures fill when the chain is awaited** — don't read `.Value` while *building* a later step in the same
  chain; `await` the producer first.
- `Trapture<T>` converts implicitly (transparent flow); `Capture<T>` needs `.Value` (explicit read).
- Use **[`Ensure`](/guides/ensure/)** to derive-guard-capture an id in one step.

## Stage & fixtures

- Compose containers **once** (`TestStageSetup.Create`); **`EnterStage()`** per test; dispose after.
- **One fixture per SUT** — a type can't be both the real SUT and a mock in one composition.
- Base fixture exposes `Arrange` / `Act` / `Inspect` properties over the current stage.

## Assertions & effects

- **Result assertions → a [value scope](/guides/inspect-scopes/)** (`ThenValueScope`); a **context scope** wraps
  them (e.g. `Assert.Multiple`).
- **State assertions read** the real DB/API; **interaction assertions** verify mocks (`Received`, `Verify`).
- **Async effects → [poll](/guides/eventually-consistent/)** (`ApiClientEventually`, a probe). Never
  `Task.Delay` a fixed guess.
- **[Snapshot](/guides/verify-snapshots/)** whole objects with `Verify(capture)` — keep values deterministic
  ([`Clock`/`Ids`](/guides/deterministic-time-ids/)) so snapshots are stable.

## Determinism

- Depend on `IDateTimeProvider` / `IIdGenerator`, not `DateTime.UtcNow` / `Guid.NewGuid()`.
- Arrange them with `Clock(…)` / `Ids(…)` and shared fixed constants.
