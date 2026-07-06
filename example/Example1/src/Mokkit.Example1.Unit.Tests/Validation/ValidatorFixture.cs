using Microsoft.Extensions.DependencyInjection;
using Mokkit.Example1.Application;
using Mokkit.Example1.Unit.Tests.Containers;

namespace Mokkit.Example1.Unit.Tests.Validation;

/// <summary>SUT: the real FluentValidation validator (from the application layer); no substitutes needed.</summary>
public sealed class ValidatorFixture : BaseStageFixture
{
    protected override void ConfigureSubstitutes(ISubstituteCollection substitutes)
    {
        // The validator has no dependencies to fake.
    }

    protected override void ConfigureServices(IServiceCollection services) =>
        services.AddApplicationLayer();
}
