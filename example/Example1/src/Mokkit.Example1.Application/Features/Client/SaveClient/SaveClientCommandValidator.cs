using FluentValidation;

namespace Mokkit.Example1.Application.Features.Client.SaveClient;

/// <summary>
/// Validator for SaveClientCommand.
/// </summary>
internal sealed class SaveClientCommandValidator : AbstractValidator<SaveClientCommand>
{
    public SaveClientCommandValidator()
    {
        RuleFor(x => x.ClientData).NotNull()
            .WithMessage("Client data is required");

        RuleFor(x => x.ClientData.Name)
            .NotEmpty()
            .WithMessage("Client name is required")
            .MaximumLength(255)
            .WithMessage("Client name cannot exceed 255 characters");

        RuleFor(x => x.ClientData.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.ClientData.Phone)
            .NotEmpty()
            .WithMessage("Phone is required")
            .MaximumLength(50)
            .WithMessage("Phone cannot exceed 50 characters");

        RuleFor(x => x.ClientData.Status)
            .GreaterThan(0)
            .WithMessage("Status must be a valid value");

        When(x => x.Operation == SaveOperationKind.Update, () =>
        {
            RuleFor(x => x.ClientData.Id)
                .NotNull()
                .NotEqual(Guid.Empty)
                .WithMessage("Client ID is required for update operations");
        });
    }
}
