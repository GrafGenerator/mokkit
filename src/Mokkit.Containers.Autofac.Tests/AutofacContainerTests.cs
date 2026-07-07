using System.Threading.Tasks;
using Mokkit.Containers;
using Mokkit.Containers.Common;
using Mokkit.Containers.NSubstitute;
using Mokkit.Suite;
using Xunit;
using global::Autofac;
using global::NSubstitute;

namespace Mokkit.Containers.Autofac.Tests;

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

public class AutofacContainerTests
{
    [Fact]
    public async Task ResolvesRegisteredService_WithConstructorInjection()
    {
        var builder = new AutofacContainerBuilder()
            .UseInit(cb =>
            {
                cb.RegisterType<RealCalculator>().As<ICalculator>();
                cb.RegisterType<CalculatorClient>().AsSelf();
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

        var autofacBuilder = new AutofacContainerBuilder()
            .UseInit(cb =>
            {
                cb.RegisterType<CalculatorClient>().AsSelf();
                return Task.CompletedTask;
            })
            .UsePreBuild<IMockCollection<object>>((cb, mocks) =>
            {
                foreach (var registration in mocks.Registrations)
                {
                    cb.ResolveFromStage(registration.InnerType);
                }

                return Task.CompletedTask;
            });

        var setup = await TestStageSetup.Create(new IDependencyContainerBuilder[]
        {
            substituteBuilder,
            autofacBuilder
        });
        var stage = setup.EnterStage();

        stage.Execute<ICalculator>(calc => calc.Add(10, 20).Returns(30));
        var result = stage.Execute<CalculatorClient, int>(client => client.Compute(10, 20));

        Assert.Equal(30, result);
        stage.Execute<ICalculator>(calc => calc.Received().Add(10, 20));
    }
}
