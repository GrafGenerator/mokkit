using FluentValidation;
using FluentValidation.Results;
using Mokkit.Example1.Application.Features.Client.SaveClient;

namespace Mokkit.Example1.Unit.Tests.Validation;

/// <summary>
/// Unit tests for <c>SaveClientCommandValidator</c>, resolved from the Mokkit stage (no mocks needed).
/// Same target as the integration validator tests, but driven with xUnit <c>[Theory]</c> + Shouldly.
/// </summary>
public sealed class SaveClientCommandValidatorTests : BaseUnitTest
{
    public SaveClientCommandValidatorTests(StageFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Create_WithValidData_Passes()
    {
        var command = SaveClientCommands.Create();

        var result = await Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Name_IsRequired(string? name)
    {
        var command = SaveClientCommands.Create(d => d.Name = name!);

        var result = await Validate(command);

        result.ShouldBeInvalidFor("ClientData.Name");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public async Task Email_MustBeValid(string? email)
    {
        var command = SaveClientCommands.Create(d => d.Email = email!);

        var result = await Validate(command);

        result.ShouldBeInvalidFor("ClientData.Email");
    }

    [Fact]
    public async Task Phone_IsRequired()
    {
        var command = SaveClientCommands.Create(d => d.Phone = "");

        var result = await Validate(command);

        result.ShouldBeInvalidFor("ClientData.Phone");
    }

    [Fact]
    public async Task Status_MustBePositive()
    {
        var command = SaveClientCommands.Create(d => d.Status = 0);

        var result = await Validate(command);

        result.ShouldBeInvalidFor("ClientData.Status");
    }

    [Fact]
    public async Task Update_WithoutId_Fails()
    {
        var command = SaveClientCommands.Update(Guid.Empty);

        var result = await Validate(command);

        result.ShouldBeInvalidFor("ClientData.Id");
    }

    private Task<ValidationResult> Validate(SaveClientCommand command)
        => Stage.ExecuteAsync<IValidator<SaveClientCommand>, ValidationResult>(
            validator => validator.ValidateAsync(command));
}

internal static class ValidationResultShouldExtensions
{
    public static void ShouldBeInvalidFor(this ValidationResult result, string propertyName)
    {
        result.IsValid.ShouldBeFalse();
        result.Errors
            .Select(e => e.PropertyName)
            .ShouldContain(propertyName);
    }
}
