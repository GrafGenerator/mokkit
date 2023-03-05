using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mokkit.Capture.Containers;
using Mokkit.Capture.Suite;
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

    private async Task BuildServices(IServiceCollection services)
    {
        services.AddScoped<IService1, Service1>();
        services.AddSingleton<IService2, Service2>();
        services.AddScoped<SampleActor>();
    }
}