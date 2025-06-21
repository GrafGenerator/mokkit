using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mokkit.Arrange;
using Mokkit.Containers;
using Mokkit.Containers.Microsoft.Extensions.DependencyInjection;
using Mokkit.Containers.Moq;
using Mokkit.Inspect;
using Mokkit.Suite;
using Mokkit.Playground.SampleScenery;
using Moq;
using NUnit.Framework;

namespace Mokkit.Playground.CaptureTests;

public class BaseParallelPlayground
{
    private static TestStageSetup _setup = null!;

    protected TestStage Stage { get; private set; }

    [OneTimeSetUp]
    public static async Task OneTimeSetUp()
    {
        var mockContainerBuilder = new MoqContainerBuilder()
            .UseInit(BuildMocks);

        var serviceProviderContainerBuilder = new ServiceProviderContainerBuilder()
            .UseInit(BuildServices)
            .UsePreBuild<IMockCollection<Mock>>(InjectMocks);

        var builders = new IDependencyContainerBuilder[]
        {
            mockContainerBuilder,
            serviceProviderContainerBuilder,
        };

        _setup = await TestStageSetup.Create(builders);
    }
    
    [SetUp]
    public async Task SetUp()
    {
        Stage = _setup.EnterStage();
    }

    [TearDown]
    public async Task TearDown()
    {
        Stage.Dispose();
    }
    
    protected ITestArrange Arrange => Stage.Arrange();

    protected ITestInspect Inspect => Stage.Inspect();

    private static async Task BuildServices(IServiceCollection services)
    {
        services.AddSingleton<IService1, Service1>();
        services.AddSingleton<IService2, Service2>();
        services.AddScoped<SampleActor>();
        services.AddScoped<SampleScopedActor>();
    }

    private static async Task BuildMocks(IMockCollection<Mock> mocks)
    {
        mocks.AddMock<IService3>(() => new Mock<IService3>());
        mocks.AddMock<IService4>(() => new Mock<IService4>());
    }

    private static Task InjectMocks(IServiceCollection services, IMockCollection<Mock> mockCollection)
    {
        foreach (var registration in mockCollection.Registrations)
        {
            services.ResolveFromStage(registration.InnerType);
        }
        
        return Task.CompletedTask;
    }
}