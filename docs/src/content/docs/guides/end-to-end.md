---
title: Full black-box E2E with Testcontainers
description: Boot the whole system in Docker — API, Postgres, Redis, Kafka — and drive it through its public surface with the same Arrange/Act/Inspect.
---

An end-to-end test uses **no mocks at all**: the real API, database, cache and message bus all run in Docker,
and the test drives the system through its public HTTP/Kafka surface. The remarkable part is that the test
*body* is identical to a [unit test](/guides/unit-mocked-dependency/) — only the vocabulary underneath resolves
real clients instead of doubles.

## The stack

A collection fixture boots everything once on a shared Docker network — Postgres, Redis, Kafka, and the **real
API built from its Dockerfile**:

```csharp
_apiImage = new ImageFromDockerfileBuilder()
    .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
    .WithDockerfile("src/Mokkit.Example1.Api/Dockerfile")
    .WithName($"mokkit-example1-api-e2e-{Guid.NewGuid():N}")
    .Build();
await _apiImage.CreateAsync();

_api = new ContainerBuilder()
    .WithImage(_apiImage)
    .WithNetwork(_network)
    .WithEnvironment("Database__Primary", $"Host=postgres;Port=5432;Database={PgDatabase};...")
    .WithEnvironment("Kafka__BootstrapServers", InNetworkKafkaListener)
    .WithPortBinding(8080, true)
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilHttpRequestIsSucceeded(r => r.ForPath("/health").ForPort(8080)))
    .Build();
await _api.StartAsync();
```

The Mokkit stage then holds **only external clients** pointed at the running stack — this is a perfect fit for
the dependency-free [Bag container](/guides/bag-container/): no DI, no mocks, just pre-built instances.

```csharp
var external = new BagContainerBuilder().UseInit(bag =>
{
    bag.AddInstance(new HttpClient { BaseAddress = apiBaseAddress });
    bag.AddInstance<IProducer<string, string>>(BuildKafkaProducer(kafkaBootstrap));
    bag.AddInstance(new KafkaProbe(kafkaBootstrap));               // reads topics, to assert on events
    bag.AddFactory(() => new ExampleContext(NpgsqlOptions(_pgConnectionString)));  // read the DB directly
    return Task.CompletedTask;
});

_setup = await TestStageSetup.Create(external);
```

## Per-test isolation

Each test enters a fresh stage; the database is Respawn-reset afterwards. The base fixture keeps the test class
clean and exposes the phase properties:

```csharp
public abstract class BaseE2ETest : IAsyncLifetime
{
    private readonly E2EStack _stack;
    protected BaseE2ETest(E2EStack stack) => _stack = stack;

    protected TestStage Stage { get; private set; } = null!;
    protected ITestArrange Arrange => Stage.Arrange();
    protected ITestAct     Act     => Stage.Act();
    protected ITestInspect Inspect => Stage.Inspect();

    public Task InitializeAsync() { Stage = _stack.EnterStage(); return Task.CompletedTask; }
    public async Task DisposeAsync() { await _stack.ResetDatabaseAsync(); Stage.Dispose(); }
}
```

## Vocabulary: real operations, real observations

Act verbs perform real HTTP calls; inspect verbs read the API, the database and Kafka:

```csharp
// Act — a real POST, returning the write-result artifact.
public static ITestAct<ClientWriteResult> CreateClient(this ITestAct act, params ClientFieldFn[] fields) =>
    act.Returning(host => host.ExecuteAsync<HttpClient, ClientWriteResult>(
        http => ClientApi.CreateAsync(http, ArrangeClientApi.Build(fields))));

// Inspect — read the row straight from Postgres.
public static ITestInspect DbClient(this ITestInspect inspect, Guid clientId, Action<Client?> assert) =>
    inspect.Then(async host => await host.ExecuteAsync<ExampleContext>(async db =>
        assert(await db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clientId))));

// Inspect — confirm the service published an event keyed by the client id.
public static ITestInspect EventPublished(this ITestInspect inspect, string topic, Guid clientId) =>
    inspect.Then(async host => await host.ExecuteAsync<KafkaProbe>(async probe =>
        (await probe.SawMessageKeyed(topic, clientId.ToString(), Timeout)).ShouldBeTrue()));
```

## The test

```csharp
[Fact]
public async Task Create_ViaApi_IsRetrievable_Persisted_AndAnnounced()
{
    // ACT — create the client through the public API.
    var result = await Act.CreateClient(WithName("Acme Corporation"), WithEmail("acme@e2e.test"));

    // INSPECT — assert the result, capture its id, then observe the three downstream effects concurrently.
    await Inspect
        .WriteResult(result).Created()
        .Ensure(result, r => r.ClientId, out var clientId)
        .ThenAll(
            b => b.ApiClient(clientId, c => c.Name.ShouldBe("Acme Corporation")),
            b => b.DbClient(clientId, c => c.ShouldNotBeNull()),
            b => b.EventPublished("clients.created", clientId));
}
```

No part of the system is faked: the assertion passes only if the HTTP endpoint, the persistence layer and the
Kafka publisher all did their jobs. Because effects are asynchronous, E2E leans on
[eventually-consistent assertions](/guides/eventually-consistent/) — and a whole lifecycle can be walked as a
[scenario](/concepts/scenarios/).

:::note[The Dockerfile's runtime user]
The API image runs as the non-root user the .NET base image provides — `USER $APP_UID`. (Older templates ran
`adduser`, which the .NET 10 base image no longer ships.)
:::

## Next

- **[Async / eventually-consistent assertions](/guides/eventually-consistent/)** — `ApiClientEventually`, polling.
- **[Scenario tests](/concepts/scenarios/)** — walk a create → update → suspend lifecycle in one test.
- **[The Bag container](/guides/bag-container/)** — the dependency-free holder used here.
