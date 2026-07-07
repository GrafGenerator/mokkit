using System.Threading.Tasks;
using Mokkit.Containers;
using Mokkit.Containers.Common;
using Mokkit.Containers.NSubstitute;
using Mokkit.Suite;
using Xunit;
using global::Microsoft.Extensions.DependencyInjection;
using global::NSubstitute;

namespace Mokkit.Containers.Microsoft.Extensions.DependencyInjection.Tests;

public interface ICalculator
{
    int Add(int a, int b);
}

public sealed class RealCalculator : ICalculator
{
    public int Add(int a, int b) => a + b;
}

public sealed class CalculatorClient
{
    private readonly ICalculator _calculator;

    public CalculatorClient(ICalculator calculator) => _calculator = calculator;

    public int Compute(int a, int b) => _calculator.Add(a, b);
}

public class ServiceProviderContainerTests
{
    [Fact]
    public async Task ResolvesRegisteredService_WithConstructorInjection()
    {
        var builder = new ServiceProviderContainerBuilder()
            .UseInit(services =>
            {
                services.AddScoped<ICalculator, RealCalculator>();
                services.AddScoped<CalculatorClient>();
                return Task.CompletedTask;
            });

        var setup = await TestStageSetup.Create(builder);
        var stage = setup.EnterStage();

        var result = stage.Execute<CalculatorClient, int>(client => client.Compute(2, 3));

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task ResolvesMockFromStage_ViaBridge()
    {
        var substituteBuilder = new NSubstituteContainerBuilder()
            .UseInit(mocks =>
            {
                mocks.AddSubstitute<ICalculator>();
                return Task.CompletedTask;
            });

        var serviceProviderBuilder = new ServiceProviderContainerBuilder()
            .UseInit(services =>
            {
                services.AddScoped<CalculatorClient>();
                return Task.CompletedTask;
            })
            .UsePreBuild<IMockCollection<object>>((services, mocks) =>
            {
                foreach (var registration in mocks.Registrations)
                {
                    services.ResolveFromStage(registration.InnerType);
                }

                return Task.CompletedTask;
            });

        var setup = await TestStageSetup.Create(new IDependencyContainerBuilder[]
        {
            substituteBuilder,
            serviceProviderBuilder
        });
        var stage = setup.EnterStage();

        stage.Execute<ICalculator>(calc => calc.Add(10, 20).Returns(30));
        var result = stage.Execute<CalculatorClient, int>(client => client.Compute(10, 20));

        Assert.Equal(30, result);
        stage.Execute<ICalculator>(calc => calc.Received().Add(10, 20));
    }
}
