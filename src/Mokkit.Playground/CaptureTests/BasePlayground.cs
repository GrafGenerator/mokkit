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

public class BasePlayground
{
    protected TestStage Stage = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
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

        Stage = await TestStage.Create(builders);
    }

    private Task InjectMocks(IServiceCollection services, IMockCollection<Mock> mockCollection)
    {
        foreach (var registration in mockCollection.Registrations)
        {
            services.AddSingleton(registration.InnerType, registration.Mock.Object);
        }
        
        
        return Task.CompletedTask;
    }

    protected ITestArrange Arrange => Stage.Arrange();

    protected ITestInspect Inspect => Stage.Inspect();

    protected virtual Task BuildAdditionalServices(IServiceCollection services)
    {
        return Task.CompletedTask;
    }

    protected virtual Task BuildAdditionalMocks(IMockCollection<Mock> mocks)
    {
        return Task.CompletedTask;
    }

    private async Task BuildServices(IServiceCollection services)
    {
        services.AddSingleton<IService1, Service1>();
        services.AddSingleton<IService2, Service2>();
        services.AddScoped<SampleActor>();

        await BuildAdditionalServices(services);
    }

    private async Task BuildMocks(IMockCollection<Mock> mocks)
    {
        mocks.AddMock<IService3>(new Mock<IService3>());
        mocks.AddMock<IService4>(new Mock<IService4>());

        await BuildAdditionalMocks(mocks);
    }
}