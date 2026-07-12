---
title: Snapshot assertions with Verify
description: Assert a whole object at once by snapshotting it — wrapped in a reusable inspect verb with project-wide settings.
---

Sometimes the clearest assertion is "the whole thing looks like *this*" — every field of a persisted entity, not
a dozen hand-written `Assert.That`s. [Verify](https://github.com/VerifyTests/Verify) snapshots the object to a
committed `.verified.txt` file and fails when it drifts. Mokkit wraps it in a normal inspect verb, so a snapshot
is just another word in your vocabulary.

## A `Verify` inspect verb

Centralise the Verify settings once, then expose a verb that snapshots any value (with `[CallerFilePath]` so the
snapshot lands next to the test):

```csharp
public static class VerifierSetup
{
    public static VerifySettings Default(Action<VerifySettings>? configure = null)
    {
        var settings = new VerifySettings();
        settings.DontScrubGuids();               // our ids are deterministic — show them literally
        settings.DontIgnoreEmptyCollections();
        configure?.Invoke(settings);
        return settings;
    }
}

public static ITestInspect Verify<T>(
    this ITestInspect inspect, Capture<T> capture,
    Action<VerifySettings>? configure = null,
    [CallerFilePath] string sourceFile = "") =>
    inspect.Then(async _ => await Verifier.Verify(capture.Value, VerifierSetup.Default(configure), sourceFile));
```

## Using it in a chain

Read the row into a capture, then hand that capture to `Verify` — the snapshot covers every field at once:

```csharp
await Inspect
    .SaveResult(result).IsSuccess(ArrangeClient.FixedClientId)
    .DbClientById(clientId, out var saved, c => Assert.That(c, Is.Not.Null))
    .Verify(saved)                          // snapshot the whole persisted entity
    .CacheUpdated(clientId)
    .EventPublished(clientId, "created");
```

Because the [clock and ids are deterministic](/guides/deterministic-time-ids/), the snapshot is stable run to
run — `CreatedAt` is always the fixed time, the id is always the fixed Guid — so a diff means a *real* change,
not noise.

## The workflow

1. First run writes `<TestClass>.<TestName>.received.txt` and the test "fails".
2. Review it. If it's right, accept it — rename to `.verified.txt` (or use a clipboard/accept tool) and
   **commit** it.
3. From then on, any drift from the committed snapshot fails the test with a diff.

:::note[Scrub what you can't control]
`DontScrubGuids()` is only safe *because* ids are pinned. For values you genuinely can't fix (a real
timestamp, a random token), let Verify scrub them to ordered placeholders (`Guid_1`, `DateTime_1`) instead — the
snapshot then asserts *shape*, not the volatile value.
:::

## Next

- **[Deterministic time & ids](/guides/deterministic-time-ids/)** — the prerequisite for stable snapshots.
- **[Value & context scopes](/guides/inspect-scopes/)** — the `SaveResult(...).IsSuccess()` scope above.
