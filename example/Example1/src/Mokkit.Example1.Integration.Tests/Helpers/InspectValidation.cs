using System.Linq;
using FluentValidation.Results;
using Mokkit.Inspect;

namespace Mokkit.Example1.Integration.Tests;

/// <summary>
/// Inspect building blocks for FluentValidation results, used by validator tests.
/// </summary>
public static class InspectValidation
{
    /// <summary>
    /// Opens a value scope over a <see cref="ValidationResult"/> so chained assertions run
    /// inside a single <c>Assert.Multiple</c> block.
    /// </summary>
    public static ITestInspectScope<ValidationResult> ValidationResult(
        this ITestInspect inspect,
        ValidationResult result)
    {
        return inspect.ThenValueScope(result, async (_, execute) =>
        {
            await Assert.MultipleAsync(async () => await execute());
        });
    }

    /// <summary>Asserts the result is valid.</summary>
    public static ITestInspectScope<ValidationResult> IsValid(
        this ITestInspectScope<ValidationResult> inspect)
    {
        return inspect.Then((result, _) => Assert.That(result.IsValid, Is.True,
            () => "Expected validation to pass, errors: " +
                  string.Join(", ", result.Errors.Select(e => e.ErrorMessage))));
    }

    /// <summary>Asserts the result is invalid and contains an error for the given property.</summary>
    public static ITestInspectScope<ValidationResult> IsInvalidFor(
        this ITestInspectScope<ValidationResult> inspect,
        string propertyName)
    {
        return inspect.Then((result, _) =>
        {
            Assert.That(result.IsValid, Is.False, "Expected validation to fail");
            Assert.That(
                result.Errors.Select(e => e.PropertyName),
                Has.Some.EqualTo(propertyName),
                () => $"Expected an error for '{propertyName}', actual: " +
                      string.Join(", ", result.Errors.Select(e => e.PropertyName)));
        });
    }
}
