using FluentValidation;
using FluentValidation.Results;
using Mokkit.Example1.Application.Features.Client.SaveClient;
using static Mokkit.Example1.Unit.Tests.Validation.ArrangeCommand;

namespace Mokkit.Example1.Unit.Tests.Validation;

/// <summary>
/// Unit tests for <c>SaveClientCommandValidator</c>, resolved from the stage. The command is built by a
/// stage arrange; the result is asserted through an <c>InspectValidation</c> value scope.
/// </summary>
public sealed class SaveClientCommandValidatorTests : BaseUnitTest<ValidatorFixture>
{
    public SaveClientCommandValidatorTests(ValidatorFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Create_WithValidData_Passes()
    {
        await Arrange.SaveCommand(out var command);

        var result = await Validate(command);

        await Inspect.Validation(result).IsValid();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Name_IsRequired(string? name)
    {
        await Arrange.SaveCommand(out var command, WithName(name));

        var result = await Validate(command);

        await Inspect.Validation(result).IsInvalidFor("ClientData.Name");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public async Task Email_MustBeValid(string? email)
    {
        await Arrange.SaveCommand(out var command, WithEmail(email));

        var result = await Validate(command);

        await Inspect.Validation(result).IsInvalidFor("ClientData.Email");
    }

    [Fact]
    public async Task Phone_IsRequired()
    {
        await Arrange.SaveCommand(out var command, WithPhone(""));

        var result = await Validate(command);

        await Inspect.Validation(result).IsInvalidFor("ClientData.Phone");
    }

    [Fact]
    public async Task Status_MustBePositive()
    {
        await Arrange.SaveCommand(out var command, WithStatus(0));

        var result = await Validate(command);

        await Inspect.Validation(result).IsInvalidFor("ClientData.Status");
    }

    [Fact]
    public async Task Update_WithoutId_Fails()
    {
        await Arrange.UpdateCommand(out var command, Guid.Empty);

        var result = await Validate(command);

        await Inspect.Validation(result).IsInvalidFor("ClientData.Id");
    }

    private Task<ValidationResult> Validate(SaveClientCommand command) =>
        Stage.ExecuteAsync<IValidator<SaveClientCommand>, ValidationResult>(
            validator => validator.ValidateAsync(command));
}
