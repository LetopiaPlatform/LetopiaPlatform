using FluentValidation;
using LetopiaPlatform.Core.DTOs.Email;

namespace LetopiaPlatform.API.Validators.User;

public class EmailConfirmRequestValidator : AbstractValidator<EmailConfirmRequest>
{
    public EmailConfirmRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.")
            .MaximumLength(512).WithMessage("Token is invalid.");
    }
}
