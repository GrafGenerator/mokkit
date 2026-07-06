using FluentValidation;
using FluentValidation.Results;
using Mokkit.Example1.Application.Features.Client.SaveClient;
using static Mokkit.Example1.Integration.Tests.Features.Client.SaveClient.ArrangeSaveClient;

namespace Mokkit.Example1.Integration.Tests.Features.Client.SaveClient;

[TestFixture]
public sealed class SaveClientCommandValidatorTests : BaseIntegrationTest
{
    [Test]
    public async Task Create_WithValidData_Passes()
    {
        await Arrange.CreateClientCommand(out var command);

        var result = await Validate(command);

        await Inspect.ValidationResult(result).IsValid();
    }

    [TestCase("", TestName = "Name_Empty_Fails")]
    [TestCase(null, TestName = "Name_Null_Fails")]
    public async Task Name_Required(string? name)
    {
        await Arrange.CreateClientCommand(out var command, WithName(name));

        var result = await Validate(command);

        await Inspect.ValidationResult(result).IsInvalidFor("ClientData.Name");
    }

    [TestCase("plainaddress", TestName = "Email_Malformed_Fails")]
    [TestCase("", TestName = "Email_Empty_Fails")]
    public async Task Email_MustBeValid(string? email)
    {
        await Arrange.CreateClientCommand(out var command, WithEmail(email));

        var result = await Validate(command);

        await Inspect.ValidationResult(result).IsInvalidFor("ClientData.Email");
    }

    [Test]
    public async Task Phone_Required()
    {
        await Arrange.CreateClientCommand(out var command, WithPhone(""));

        var result = await Validate(command);

        await Inspect.ValidationResult(result).IsInvalidFor("ClientData.Phone");
    }

    [Test]
    public async Task Status_MustBePositive()
    {
        await Arrange.CreateClientCommand(out var command, WithStatus(0));

        var result = await Validate(command);

        await Inspect.ValidationResult(result).IsInvalidFor("ClientData.Status");
    }

    [Test]
    public async Task Update_WithoutId_Fails()
    {
        await Arrange.UpdateClientCommand(out var command, Guid.Empty);

        var result = await Validate(command);

        await Inspect.ValidationResult(result).IsInvalidFor("ClientData.Id");
    }

    private Task<ValidationResult> Validate(SaveClientCommand command)
        => Stage.ExecuteAsync<IValidator<SaveClientCommand>, ValidationResult>(
            validator => validator.ValidateAsync(command));
}
