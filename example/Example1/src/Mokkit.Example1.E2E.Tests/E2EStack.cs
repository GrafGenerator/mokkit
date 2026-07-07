using Confluent.Kafka;
using Confluent.Kafka.Admin;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using Microsoft.EntityFrameworkCore;
using Mokkit.Containers.Bag;
using Mokkit.Example1.Db.Postgres;
using Mokkit.Suite;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Mokkit.Example1.E2E.Tests;

/// <summary>
/// Boots the full system in Docker — Postgres, Kafka, Redis and the real API (built from its Dockerfile) —
/// on one network, then builds a Mokkit stage whose container resolves <b>external</b> clients pointed at
/// the running stack (HttpClient, Kafka producer, a Kafka probe, the database). Shared once per test run via
/// an xUnit collection fixture; data is Respawn-reset between tests.
/// </summary>
public sealed class E2EStack : IAsyncLifetime
{
    private const string PgDatabase = "example1";
    private const string PgUsername = "postgres";
    private const string PgPassword = "postgres";
    private const string ServiceSchema = "example1";
    private const string InNetworkKafkaListener = "kafka:19092";

    private INetwork _network = null!;
    private PostgreSqlContainer _postgres = null!;
    private RedisContainer _redis = null!;
    private KafkaContainer _kafka = null!;
    private IFutureDockerImage _apiImage = null!;
    private IContainer _api = null!;

    private TestStageSetup _setup = null!;
    private Respawner _respawner = null!;
    private string _pgConnectionString = null!;

    public async Task InitializeAsync()
    {
        _network = new NetworkBuilder().Build();
        await _network.CreateAsync();

        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithImagePullPolicy(PullPolicy.Missing)
            .WithNetwork(_network)
            .WithNetworkAliases("postgres")
            .WithDatabase(PgDatabase)
            .WithUsername(PgUsername)
            .WithPassword(PgPassword)
            .Build();

        _redis = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithImagePullPolicy(PullPolicy.Missing)
            .WithNetwork(_network)
            .WithNetworkAliases("redis")
            .Build();

        _kafka = new KafkaBuilder()
            .WithImage("confluentinc/cp-kafka:7.6.0")
            .WithImagePullPolicy(PullPolicy.Missing)
            .WithNetwork(_network)
            .WithNetworkAliases("kafka")
            .WithListener(InNetworkKafkaListener) // extra advertised listener for the API container
            .WithEnvironment("KAFKA_AUTO_CREATE_TOPICS_ENABLE", "true") // else publishing to a new topic stalls
            .Build();

        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync(), _kafka.StartAsync());

        // Pre-create the topics before the API starts so its consumer binds to an existing topic
        // immediately (otherwise librdkafka can take a metadata-refresh interval to notice a new one).
        var kafkaBootstrap = StripScheme(_kafka.GetBootstrapAddress());
        await CreateTopicsAsync(kafkaBootstrap, "clients.status-changed", "clients.created", "clients.updated");

        _apiImage = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile("src/Mokkit.Example1.Api/Dockerfile")
            .WithName($"mokkit-example1-api-e2e-{Guid.NewGuid():N}")
            .WithCleanUp(true)
            .Build();
        await _apiImage.CreateAsync();

        _api = new ContainerBuilder()
            .WithImage(_apiImage)
            .WithNetwork(_network)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("ASPNETCORE_URLS", "http://+:8080")
            .WithEnvironment("Database__Primary",
                $"Host=postgres;Port=5432;Database={PgDatabase};Username={PgUsername};Password={PgPassword};")
            .WithEnvironment("ConnectionStrings__Redis", "redis:6379")
            .WithEnvironment("Kafka__BootstrapServers", InNetworkKafkaListener)
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(request => request.ForPath("/health").ForPort(8080)))
            .Build();
        await _api.StartAsync();

        _pgConnectionString = _postgres.GetConnectionString();
        var apiBaseAddress = new UriBuilder("http", _api.Hostname, _api.GetMappedPublicPort(8080)).Uri;

        // The lightweight Bag container just holds the pre-built external clients this suite resolves — no
        // Microsoft.Extensions.DependencyInjection dependency, no auto-wiring, no options indirection.
        var external = new BagContainerBuilder().UseInit(bag =>
        {
            bag.AddInstance(new HttpClient { BaseAddress = apiBaseAddress });
            bag.AddInstance<IProducer<string, string>>(
                new ProducerBuilder<string, string>(new ProducerConfig
                {
                    BootstrapServers = kafkaBootstrap,
                    MessageTimeoutMs = 10000 // fail fast instead of librdkafka's 5-minute default
                }).Build());
            bag.AddInstance(new KafkaProbe(kafkaBootstrap));

            // Created fresh per stage and disposed when the stage ends (matching the previous scoped DbContext lifetime).
            bag.AddFactory(() => new ExampleContext(
                new DbContextOptionsBuilder<ExampleContext>().UseNpgsql(_pgConnectionString).Options));

            return Task.CompletedTask;
        });

        _setup = await TestStageSetup.Create(external);

        await using var connection = new NpgsqlConnection(_pgConnectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            SchemasToInclude = [ServiceSchema],
            TablesToIgnore = [new Table(ServiceSchema, "__EFMigrationsHistory")],
            DbAdapter = DbAdapter.Postgres
        });
    }

    public TestStage EnterStage() => _setup.EnterStage();

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_pgConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public async Task DisposeAsync()
    {
        if (_api is not null) await _api.DisposeAsync();
        if (_apiImage is not null) await _apiImage.DisposeAsync();
        if (_kafka is not null) await _kafka.DisposeAsync();
        if (_redis is not null) await _redis.DisposeAsync();
        if (_postgres is not null) await _postgres.DisposeAsync();
        if (_network is not null) await _network.DeleteAsync();
    }

    private static async Task CreateTopicsAsync(string bootstrap, params string[] topics)
    {
        using var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrap }).Build();
        try
        {
            await admin.CreateTopicsAsync(topics.Select(name => new TopicSpecification
            {
                Name = name,
                NumPartitions = 1,
                ReplicationFactor = 1
            }));
        }
        catch (CreateTopicsException ex) when (ex.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            // Already created on a previous run — fine.
        }
    }

    private static string StripScheme(string bootstrap) =>
        bootstrap.Contains("://") ? bootstrap[(bootstrap.IndexOf("://", StringComparison.Ordinal) + 3)..] : bootstrap;
}
