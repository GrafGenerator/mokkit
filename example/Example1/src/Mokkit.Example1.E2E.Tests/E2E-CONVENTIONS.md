# Mokkit E2E / automation-test conventions

This is the third Mokkit layer. It is **black-box**: it boots the *real* service and its dependencies in
Docker and drives them from the outside (HTTP + Kafka), asserting against the running system. It reuses
the **same Mokkit Arrange/Act/Inspect DSL** as the unit and integration suites — only the helpers differ:
here the stage resolves **external clients** instead of in-process services.

| | Unit | Integration | **E2E (this)** |
|---|---|---|---|
| Framework | xUnit | NUnit | **xUnit** |
| Boundary | one class, mocks | handler + real DB, mocked Kafka | **whole service in Docker, nothing mocked** |
| Stage resolves | real SUT + NSubstitute fakes | real services + Moq fakes | **HttpClient, Kafka producer/probe, DbContext** |
| Infra | none | local Postgres | **Testcontainers: API + Postgres + Kafka + Redis** |

---

## 1. The stack ([E2EStack](E2EStack.cs))

An xUnit collection fixture (built once per run) starts, on a shared Docker network:
- **Postgres**, **Redis**, **Kafka** (Testcontainers modules), and
- the **API itself**, built from [its Dockerfile](../Mokkit.Example1.Api/Dockerfile) and env-wired to the
  others (`Database__Primary`, `ConnectionStrings__Redis`, `Kafka__BootstrapServers`), with readiness
  gated on `GET /health`.

It then builds the Mokkit stage via a dependency-free `BagContainerBuilder` that **holds external clients**
pointed at the running stack (`HttpClient`, `IProducer<string,string>`, `KafkaProbe`, and a per-stage
`ExampleContext` factory) — no DI framework needed, since nothing here is auto-wired. [`BaseE2ETest`](BaseE2ETest.cs)
enters a fresh stage per test and Respawn-resets the database afterwards (ignoring `__EFMigrationsHistory`).

### Kafka dual-listener
The broker advertises **two** listeners: an in-network one (`kafka:19092`, used by the API container) via
`KafkaBuilder.WithListener(...)`, and the host one (`GetBootstrapAddress()`, used by the test process).
A single advertised address can't serve both vantage points — see the plan/notes for the full rationale.

## 2. Everything is arrange / act / inspect — each step a real operation
No raw `HttpClient`/`ProduceAsync`/SQL or asserts in test bodies. Helpers live in [Clients/](Clients/), one
vocabulary file per phase:
- **Arranges** ([ArrangeClientApi](Clients/ArrangeClientApi.cs)) perform real operations: `NewClient(out id, …)`
  → `POST`. Field mutators (`WithName`/`WithEmail`/`WithStatus`) compose a request over valid defaults.
- **Acts** ([ActClientApi](Clients/ActClientApi.cs)) are the operations under test, expressed as first-class
  Act vocabulary on `ITestAct` (`Act` is a phase, symmetric with Arrange/Inspect, exposed as a property on
  `BaseE2ETest`): `Act.CreateClient(…)` / `Act.UpdateClient(id, …)` return a `ClientWriteResult`;
  `Act.ProduceStatusChanged(id, message)` is a void act that emits a Kafka message.
- **Inspects** ([InspectClientApi](Clients/InspectClientApi.cs)) read outcomes back: `ApiClient(id, …)` /
  `ApiClientEventually(id, until)` (GET, with polling for async outcomes), `DbClient(id, …)` (direct DB),
  `EventPublished(topic, id)` (Kafka probe).

A test can also be a whole **scenario** — a sequence of Arrange/Act/Inspect blocks walking a lifecycle
(create → rename → suspend), see [ClientLifecycleScenarioTests](Clients/ClientLifecycleScenarioTests.cs).

## 3. Arrange/Act produce artifacts; Inspect only observes (reads OK, mutations not)

This is the load-bearing rule that keeps AAA honest — it applies to **all three suites**.

- **Act** performs the single action under test and produces an artifact — either **returning** it
  (`var result = await Act.CreateClient(...)`) or, for a void act, leaving its effect to be observed
  downstream (`await Act.ProduceStatusChanged(id, message)`). An action that is a natural *precondition* may
  instead live in **Arrange**, which hands off its artifact via `out Capture<T>`.
- **Inspect only observes.** It may **read** — a `GET`, a DB query, a Kafka peek, a mock `Received` — and
  assert on the handed-over artifact. It must **never perform the mutating action under test** (POST/PUT/
  DELETE, produce a message, invoke the SUT command). *Reads in an inspect are fine; mutations are not.*
- If an inspect is doing the thing under test, lift it into `Act` and pass the result on. If an `Act`
  contains assertions, move them to `Inspect` — the `Act` should just return the artifact.

Worked examples here:
- **Create is the Act** → returns a `ClientWriteResult`
  ([CreateClientFlowTests](Clients/CreateClientFlowTests.cs)):
  ```csharp
  var result = await Act.CreateClient(WithName("Acme"), WithEmail("acme@e2e.test"));   // ACT → artifact
  await Inspect
      .WriteResult(result).Created()                     // assert the result
      .ApiClient(result.ClientId!.Value, c => …)         // observe downstream by its id
      .EventPublished("clients.created", result.ClientId!.Value);
  ```
- **Create as a precondition** → `Arrange.NewClient(out var id, …)` (the update flow needs an existing
  client first); its status check is a *setup guard*, not the assertion under test.
- **A rejected write is still an Act**: `var result = await Act.CreateClient(WithEmail("bad")); Inspect.WriteResult(result).Rejected();`
  — never a POST buried inside an inspect.
- **`ApiClientNotFound` is a legitimate inspect** — a 404 `GET` is a *read*, not a mutation.

## 4. Async outcomes → poll, don't sleep
Message-driven effects are eventually consistent. Use `ApiClientEventually(...)` / the `KafkaProbe` which
poll with a bounded timeout. Never `Task.Delay` a fixed guess.

## 5. Black-box contracts
The suite owns its wire DTOs in [Contracts/](Contracts/) — it does **not** reference the service's
internal request/response types. (Note: the API serialises the status enum as a number, so the contract's
`Status` is an `int`.) A status-change message must carry the full client record because the consumer
re-validates it before applying.

## 6. Running

Requires Docker. The first run is slow (it builds the API image).

```bash
dotnet test src/Mokkit.Example1.E2E.Tests
```

If Testcontainers can't find Docker (non-default context, e.g. Docker Desktop), point it at the socket:

```bash
export DOCKER_HOST=unix://$HOME/.docker/run/docker.sock
export TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE=/var/run/docker.sock
```

Images are pulled only when missing (`PullPolicy.Missing`); pre-pull `postgres:15-alpine`, `redis:7-alpine`,
`confluentinc/cp-kafka:7.6.0`, and `mcr.microsoft.com/dotnet/{sdk,aspnet}:10.0` to avoid slow first-run pulls.
