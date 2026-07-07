using System.Threading.Tasks;
using Mokkit.Containers;
using Mokkit.Containers.Common;
using Mokkit.Containers.Microsoft.Extensions.DependencyInjection;
using Mokkit.Suite;
using Xunit;
using global::Microsoft.Extensions.DependencyInjection;
using global::Moq;

namespace Mokkit.Containers.Moq.Tests;

public interface ICalculator
{
    int Add(int a, int b);
}

public sealed class CalculatorClient
{
    private readonly ICalculator _calculator;

    public CalculatorClient(ICalculator calculator) => _calculator = calculator;

    public int Compute(int a, int b) => _calculator.Add(a, b);
}

public class MoqContainerTests
{
    [Fact]
    public async Task Mock_IsResolvedAsWrapper_ConfiguredAndVerified()
    {
        var builder = new MoqContainerBuilder()
            .UseInit(mocks =>
            {
                mocks.AddMock<ICalculator>(() => new Mock<ICalculator>());
                return Task.CompletedTask;
            });

        var setup = await TestStageSetup.Create(builder);
        var stage = setup.EnterStage();

        stage.Execute<Mock<ICalculator>>(m => m.Setup(x => x.Add(2, 3)).Returns(5));
        var result = stage.Execute<Mock<ICalculator>, int>(m => m.Object.Add(2, 3));

        Assert.Equal(5, result);
        stage.Execute<Mock<ICalculator>>(m => m.Verify(x => x.Add(2, 3), Times.Once()));
    }

    [Fact]
    public async Task Mock_IsInjectedIntoRealGraph_ViaBag()
    {
        var moqBuilder = new MoqContainerBuilder()
            .UseInit(mocks =>
            {
                mocks.AddMock<ICalculator>(() => new Mock<ICalculator>());
                return Task.CompletedTask;
            });

        var serviceProviderBuilder = new ServiceProviderContainerBuilder()
            .UseInit(services =>
            {
                services.AddScoped<CalculatorClient>();
                return Task.CompletedTask;
            })
            .UsePreBuild<IMockCollection<Mock>>((services, mocks) =>
            {
                foreach (var registration in mocks.Registrations)
                {
                    services.ResolveFromStage(registration.InnerType);
                }

                return Task.CompletedTask;
            });

        var setup = await TestStageSetup.Create(new IDependencyContainerBuilder[]
        {
            moqBuilder,
            serviceProviderBuilder
        });
        var stage = setup.EnterStage();

        stage.Execute<Mock<ICalculator>>(m => m.Setup(x => x.Add(10, 20)).Returns(30));
        var result = stage.Execute<CalculatorClient, int>(client => client.Compute(10, 20));

        Assert.Equal(30, result);
        stage.Execute<Mock<ICalculator>>(m => m.Verify(x => x.Add(10, 20), Times.Once()));
    }
}
