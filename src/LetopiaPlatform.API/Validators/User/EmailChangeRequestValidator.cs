using FluentValidation;
using LetopiaPlatform.Core.DTOs.Email;

namespace LetopiaPlatform.API.Validators.User;

public class EmailChangeRequestValidator : AbstractValidator<EmailChangeRequest>
{
    public EmailChangeRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256).WithMessage("Email must be at most 256 characters.");
    }
}
