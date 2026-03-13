using FluentValidation;
using LetopiaPlatform.API.DTOs.Auth.Request;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.API.Validators;

public class SendCodeDtoValidator : AbstractValidator<SendCodeDto>
{
    public SendCodeDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Purpose)
            .NotEmpty().WithMessage("Purpose is required")
            .Must(p => p == nameof(VerificationPurpose.EmailVerification) || p == nameof(VerificationPurpose.PasswordReset))
            .WithMessage("Purpose must be 'EmailVerification' or 'PasswordReset'");
    }
}