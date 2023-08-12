using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mokkit.Arrange;
using Mokkit.Containers;
using Mokkit.Containers.Microsoft.Extensions.DependencyInjection;
using Mokkit.Inspect;
using Mokkit.Suite;
using Mokkit.Playground.SampleScenery;
using NUnit.Framework;

namespace Mokkit.Playground.CaptureTests;

public class BasePlayground
{
    protected TestStage Stage = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var builders = new IDependencyContainerBuilder[]{
            new MicrosoftDiContainerBuilder()
                .UseInit(BuildServices)
        };
        
        Stage = await TestStage.Create(builders);
    }

    protected ITestArrange Arrange => Stage.Arrange();

    protected ITestInspect Inspect => Stage.Inspect();

    protected virtual Task BuildAdditionalServices(IServiceCollection services)
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
}