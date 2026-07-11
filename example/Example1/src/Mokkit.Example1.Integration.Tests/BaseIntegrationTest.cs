using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Mokkit.Act;
using Mokkit.Arrange;
using Mokkit.Containers;
using Mokkit.Containers.Common;
using Mokkit.Containers.Microsoft.Extensions.DependencyInjection;
using Mokkit.Containers.Moq;
using Mokkit.Inspect;
using Mokkit.Suite;
using Moq;
using Respawn;
using Mokkit.Example1.Application;
using Mokkit.Example1.Application.Logic.Messages;
using Mokkit.Example1.Application.Logic.Persistence;
using Mokkit.Example1.Common;
using Mokkit.Example1.Db.Postgres;
using Mokkit.Example1.Db.Postgres.Options;

namespace Mokkit.Example1.Integration.Tests;

/// <summary>
/// Base class for client-domain integration tests.
///
/// It composes the <b>real</b> application (EF Core / Postgres + the application layer)
/// while replacing the outward-facing infrastructure — cache, Kafka, clock and id
/// generation — with Moq mocks that tests arrange and inspect declaratively.
///
/// A real Postgres database is created once per fixture and reset with Respawn between
/// every test, so each test starts from a clean, isolated state.
/// </summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public abstract class BaseIntegrationTest
{
    private const string ConnectionStringEnvVariable = "TEST_DATABASE";

    private static Respawner _respawner = null!;
    private static TestStageSetup _setup = null!;

    protected TestStage Stage { get; private set; } = null!;

    protected ITestArrange Arrange => Stage.Arrange();
    protected ITestAct Act => Stage.Act();
    protected ITestInspect Inspect => Stage.Inspect();

    [OneTimeSetUp]
    public static async Task OneTimeSetup()
    {
        var sessionGuid = Guid.NewGuid();

        var mockContainerBuilder = new MoqContainerBuilder()
            .UseInit(BuildMocks);

        var serviceProviderContainerBuilder = new ServiceProviderContainerBuilder()
            .UseInit(services => BuildServices(services, sessionGuid))
            .UsePreBuild<IMockCollection<Mock>>(InjectMocks);

        var builders = new IDependencyContainerBuilder[]
        {
            mockContainerBuilder,
            serviceProviderContainerBuilder,
        };

        _setup = await TestStageSetup.Create(builders);

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
    }

    [OneTimeTearDown]
    public static async Task OneTimeTearDown()
    {
        using var dbStage = _setup.EnterStage();
        await dbStage.ExecuteAsync<ExampleContext>(context => context.Database.EnsureDeletedAsync());
    }

    [SetUp]
    public void BaseSetup()
    {
        Stage = _setup.EnterStage();
    }

    [TearDown]
    public async Task BaseTearDown()
    {
        using var dbStage = _setup.EnterStage();
        await dbStage.ExecuteAsync<ExampleContext>(async context =>
        {
            await context.Database.OpenConnectionAsync();
            await _respawner.ResetAsync(context.Database.GetDbConnection());
        });

        Stage.Dispose();
    }

    private static Task BuildServices(IServiceCollection services, Guid sessionGuid)
    {
        services.AddScoped<ILogger, NullLogger>();
        services.AddScoped(typeof(ILogger<>), typeof(NullLogger<>));

        services.Configure<DatabaseOptions>(options => options.Primary = ResolveConnectionString(sessionGuid));
        services.AddPostgresDbContext();

        services.AddApplicationLayer();

        return Task.CompletedTask;
    }

    private static Task BuildMocks(IMockCollection<Mock> mocks)
    {
        mocks.AddMock<IClientCacheService>(() => new Mock<IClientCacheService>());
        mocks.AddMock<IKafkaEventPublisher>(() => new Mock<IKafkaEventPublisher>());
        mocks.AddMock<IDateTimeProvider>(() => new Mock<IDateTimeProvider>());
        mocks.AddMock<IIdGenerator>(() => new Mock<IIdGenerator>());

        return Task.CompletedTask;
    }

    private static Task InjectMocks(IServiceCollection services, IMockCollection<Mock> mockCollection)
    {
        foreach (var registration in mockCollection.Registrations)
        {
            services.ResolveFromStage(registration.InnerType);
        }

        return Task.CompletedTask;
    }

    private static string ResolveConnectionString(Guid sessionGuid)
    {
        var fromEnv = Environment.GetEnvironmentVariable(ConnectionStringEnvVariable);
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            TestContext.Progress.WriteLine(
                $"Using connection string from ENV variable '{ConnectionStringEnvVariable}': {fromEnv}");
            return fromEnv;
        }

        var connectionString =
            $"Host=localhost;Port=5432;Database=example1_test_{sessionGuid:N};Username=postgres;Password=postgres;";

        TestContext.Progress.WriteLine($"Generated connection string: {connectionString}");

        return connectionString;
    }
}
