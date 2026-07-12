---
title: Deterministic time & ids
description: Make timestamps and generated ids assertable by mocking the clock and id generator behind a small arrange verb.
---

`DateTime.UtcNow` and `Guid.NewGuid()` make a test unrepeatable — you can't assert on a value you don't control.
The fix is a two-part discipline: production code depends on **abstractions** (`IDateTimeProvider`,
`IIdGenerator`), and tests **arrange those abstractions to fixed values** through a verb.

## Fixed values + two arrange verbs

Keep the fixed constants in one place, then wrap the mock setup in `Clock` and `Ids` verbs:

```csharp
public static readonly DateTime FixedUtcNow = new(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc);
public static readonly Guid FixedClientId = Guid.Parse("11111111-1111-1111-1111-111111111111");

public static ITestArrange Clock(this ITestArrange arrange, DateTime? utcNow = null) =>
    arrange.Then(host => host.Execute<Mock<IDateTimeProvider>>(mock =>
        mock.SetupGet(x => x.UtcNow).Returns(utcNow ?? FixedUtcNow)));

public static ITestArrange Ids(this ITestArrange arrange, params Guid[] ids)
{
    var sequence = ids.Length == 0 ? new[] { FixedClientId } : ids;
    return arrange.Then(host => host.Execute<Mock<IIdGenerator>>(mock =>
    {
        var queue = new Queue<Guid>(sequence);
        // Hand out each id in turn; repeat the last once the queue runs dry.
        mock.Setup(x => x.NewId()).Returns(() => queue.Count > 1 ? queue.Dequeue() : queue.Peek());
    }));
}
```

`Ids(...)` taking `params Guid[]` means a test that creates several entities can pin each id in order —
`.Ids(firstId, secondId)` — while the common case, `.Ids()`, just uses the fixed default.

## Using them

The clock and id are arranged first, so the handler runs against known values and the result is exactly
assertable:

```csharp
await Arrange
    .Clock(ArrangeClient.FixedUtcNow)
    .Ids(ArrangeClient.FixedClientId)
    .CreateClientCommand(out var command, WithName("Acme Corporation"));

var result = await Act.SaveClient(command);

await Inspect
    .SaveResult(result).IsSuccess(ArrangeClient.FixedClientId)   // the id we pinned
    .DbClientById(ArrangeClient.FixedClientId, out var saved, c => Assert.That(c!.CreatedAt, Is.EqualTo(FixedUtcNow)));
```

Because both the created-at timestamp and the id are values *you* chose, the assertions are exact equalities —
no "roughly now", no "some Guid". That determinism is also what makes [Verify snapshots](/guides/verify-snapshots/)
stable across runs.

:::tip[The verbs read like preconditions]
`Clock` and `Ids` are cross-feature arrange vocabulary — they belong in a shared `ArrangeClient` /
`ArrangeCommon` file, not per test. See [Building your test vocabulary](/concepts/vocabulary/).
:::

## Next

- **[Snapshot assertions with Verify](/guides/verify-snapshots/)** — deterministic values make snapshots stable.
- **[Integration-test against a real database](/guides/integration-database/)** — where `Clock`/`Ids` shine.
