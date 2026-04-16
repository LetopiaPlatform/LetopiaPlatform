using FluentValidation;
using LetopiaPlatform.API.DTOs.Auth.Request;

namespace LetopiaPlatform.API.Validators;

public class VerifyEmailDtoValidator : AbstractValidator<VerifyEmailDto>
{
    public VerifyEmailDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .Length(6).WithMessage("Code must be exactly 6 digits")
            .Matches(@"^\d{6}$").WithMessage("Code must contain only digits");
    }
}