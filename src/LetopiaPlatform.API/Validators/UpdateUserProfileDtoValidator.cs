using LetopiaPlatform.API.DTOs.User;
using FluentValidation;

namespace LetopiaPlatform.API.Validators;

public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
{
    private static readonly string[] AllowedAvatarExtensions = [".jpg", ".jpeg", ".png"];

    public UpdateUserProfileDtoValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(50)
            .WithMessage("Name must be at most 50 characters.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .WithMessage("Bio must be at most 500 characters.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?\d{7,15}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Invalid phone number.");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Date of birth must be in the past.");

        RuleFor(x => x.AvatarUrl)
            .Must(file => file == null ||
                         AllowedAvatarExtensions.Contains(Path.GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase))
            .WithMessage("Only JPG, JPEG, or PNG files are allowed.")
            .Must(file => file == null || file.Length <= 5 * 1024 * 1024)
            .WithMessage("Avatar image must be less than 5MB.");
    }
}
