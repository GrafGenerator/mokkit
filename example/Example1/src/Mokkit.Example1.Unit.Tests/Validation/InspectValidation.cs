using System.Linq;
using FluentValidation.Results;
using Mokkit.Inspect;

namespace Mokkit.Example1.Unit.Tests.Validation;

/// <summary>
/// Inspect building blocks for FluentValidation results — a value scope so result assertions read as a
/// fluent chain, mirroring the integration suite.
/// </summary>
public static class InspectValidation
{
    public static ITestInspectScope<ValidationResult> Validation(this ITestInspect inspect, ValidationResult result)
    {
        return inspect.ThenValueScope(result, async (_, executeInnerFns) => await executeInnerFns());
    }

    public static ITestInspectScope<ValidationResult> IsValid(this ITestInspectScope<ValidationResult> inspect)
    {
        return inspect.Then((result, _) =>
            result.IsValid.ShouldBeTrue("expected validation to pass, errors: " +
                                        string.Join(", ", result.Errors.Select(e => e.ErrorMessage))));
    }

    public static ITestInspectScope<ValidationResult> IsInvalidFor(
        this ITestInspectScope<ValidationResult> inspect,
        string propertyName)
    {
        return inspect.Then((result, _) =>
        {
            result.IsValid.ShouldBeFalse();
            result.Errors.Select(e => e.PropertyName).ShouldContain(propertyName);
        });
    }
}
