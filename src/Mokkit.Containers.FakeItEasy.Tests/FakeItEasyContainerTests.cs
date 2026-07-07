using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mokkit.Containers;
using Mokkit.Containers.Common;
using Mokkit.Containers.Microsoft.Extensions.DependencyInjection;
using Mokkit.Suite;
using Xunit;
using global::FakeItEasy;

namespace Mokkit.Containers.FakeItEasy.Tests;

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

public class FakeItEasyContainerTests
{
    [Fact]
    public async Task Fake_IsResolvedDirectly_ConfiguredAndVerified()
    {
        var builder = new FakeItEasyContainerBuilder()
            .UseInit(mocks =>
            {
                mocks.AddFake<ICalculator>();
                return Task.CompletedTask;
            });

        var setup = await TestStageSetup.Create(builder);
        var stage = setup.EnterStage();

        stage.Execute<ICalculator>(calc => A.CallTo(() => calc.Add(2, 3)).Returns(5));
        var result = stage.Execute<ICalculator, int>(calc => calc.Add(2, 3));

        Assert.Equal(5, result);
        stage.Execute<ICalculator>(calc => A.CallTo(() => calc.Add(2, 3)).MustHaveHappened());
    }

    [Fact]
    public async Task Fake_IsInjectedIntoRealGraph_ViaBag()
    {
        var fakeBuilder = new FakeItEasyContainerBuilder()
            .UseInit(mocks =>
            {
                mocks.AddFake<ICalculator>();
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
            fakeBuilder,
            serviceProviderBuilder
        });
        var stage = setup.EnterStage();

        stage.Execute<ICalculator>(calc => A.CallTo(() => calc.Add(10, 20)).Returns(30));
        var result = stage.Execute<CalculatorClient, int>(client => client.Compute(10, 20));

        Assert.Equal(30, result);
        stage.Execute<ICalculator>(calc => A.CallTo(() => calc.Add(10, 20)).MustHaveHappened());
    }
}
