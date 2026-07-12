---
title: Integration-test against a real database
description: Exercise the real application layer against a real Postgres, with only the outward-facing edges mocked and Respawn keeping tests isolated.
---

An integration test runs the **real** application — real EF Core against a real database — and mocks only the
outward-facing edges (cache, message bus, clock, id generation). The example's integration suite does exactly
this with **Moq + Microsoft DI + Postgres + Respawn**.

## The composition

The base fixture composes a Moq container (the mocked edges) with a Microsoft DI container that wires the real
EF Core context and application layer, then bridges the mocks in:

```csharp
var mocks = new MoqContainerBuilder().UseInit(BuildMocks);

var services = new ServiceProviderContainerBuilder()
    .UseInit(s => BuildServices(s, sessionGuid))
    .UsePreBuild<IMockCollection<Mock>>(InjectMocks);       // bridge every mock into DI

_setup = await TestStageSetup.Create(mocks, services);

static Task BuildMocks(IMockCollection<Mock> mocks)
{
    mocks.AddMock<IClientCacheService>(() => new Mock<IClientCacheService>());
    mocks.AddMock<IKafkaEventPublisher>(() => new Mock<IKafkaEventPublisher>());
    mocks.AddMock<IDateTimeProvider>(() => new Mock<IDateTimeProvider>());
    mocks.AddMock<IIdGenerator>(() => new Mock<IIdGenerator>());
    return Task.CompletedTask;
}

static Task BuildServices(IServiceCollection services, Guid sessionGuid)
{
    services.Configure<DatabaseOptions>(o => o.Primary = ResolveConnectionString(sessionGuid));
    services.AddPostgresDbContext();     // the REAL EF Core context
    services.AddApplicationLayer();      // the REAL handlers/validators
    return Task.CompletedTask;
}
```

The database is a real Postgres reachable by connection string — from `docker-compose up postgres` locally, or a
service container in CI (the suite reads `TEST_DATABASE` if set, else defaults to `localhost:5432`). This is
the one difference from the [E2E suite](/guides/end-to-end/), which spins Postgres up in-process with
Testcontainers.

## Isolation with Respawn

Build the schema once and create a Respawner in a one-time setup, then reset between every test so each starts
clean:

```csharp
// One-time: create schema + a Respawner scoped to the service schema.
using var dbStage = _setup.EnterStage();
await dbStage.ExecuteAsync<ExampleContext>(async context =>
{
    await context.Database.EnsureCreatedAsync();
    await context.Database.OpenConnectionAsync();
    _respawner = await Respawner.CreateAsync(context.Database.GetDbConnection(), new RespawnerOptions
    {
        SchemasToInclude = [DatabaseConstants.ServiceSchemaName],
        DbAdapter = DbAdapter.Postgres
    });
});

// After each test: reset, then dispose the stage.
await dbStage.ExecuteAsync<ExampleContext>(async context =>
{
    await context.Database.OpenConnectionAsync();
    await _respawner.ResetAsync(context.Database.GetDbConnection());
});
```

## Vocabulary: deterministic edges + real rows

Arrange the mocked clock and id generator so timestamps and ids are assertable, and seed real rows straight
into the database:

```csharp
public static ITestArrange Clock(this ITestArrange arrange, DateTime? utcNow = null) =>
    arrange.Then(host => host.Execute<Mock<IDateTimeProvider>>(mock =>
        mock.SetupGet(x => x.UtcNow).Returns(utcNow ?? FixedUtcNow)));

// Seed an existing client through the REAL context, and capture it for assertions.
public static ITestArrange DbClient(
    this ITestArrange arrange, out Capture<Client> client, Action<Client>? mutate = null)
{
    var capture = Capture.Start(out client);
    return arrange.Then(async host => await host.ExecuteAsync<ExampleContext>(async context =>
    {
        var entity = NewDefaultClient(mutate);
        context.Clients.Add(entity);
        await context.SaveChangesAsync();
        capture.Set(entity);
    }));
}
```

## The test

Deterministic clock + id, a real create command, and assertions that span the result, the row, the cache mock
and the event mock:

```csharp
[Test]
public async Task Create_PersistsClient_UpdatesCache_AndPublishesCreatedEvent()
{
    // ARRANGE — pin the clock and id, then build a create command over the defaults.
    await Arrange
        .Clock(ArrangeClient.FixedUtcNow)
        .Ids(ArrangeClient.FixedClientId)
        .CreateClientCommand(out var command, WithName("Acme Corporation"));

    // ACT — dispatch through the real handler.
    var result = await Act.SaveClient(command);

    // INSPECT — result, then the persisted row, the cache and the published event.
    await Inspect
        .SaveResult(result).IsSuccess(ArrangeClient.FixedClientId)
        .Ensure(result, r => r.ClientId, out var clientId)
        .DbClientById(clientId, out var saved, c => Assert.That(c, Is.Not.Null))
        .CacheUpdated(clientId)
        .EventPublished(clientId, "created");
}
```

Because the handler ran for real, this test proves the whole slice — validation, EF mapping, the SQL that
lands the row — not a mock of it. Only the *edges* Mokkit can't run in-process (cache, Kafka, wall-clock,
`Guid.NewGuid()`) are doubles.

## Next

- **[Deterministic time & ids](/guides/deterministic-time-ids/)** — the `Clock`/`Ids` pattern in depth.
- **[Full black-box E2E with Testcontainers](/guides/end-to-end/)** — no mocks at all, the whole system in Docker.
- **[Snapshot assertions with Verify](/guides/verify-snapshots/)** — `.Verify(saved)` above.
