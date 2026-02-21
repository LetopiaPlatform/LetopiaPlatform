using FluentValidation;
using LetopiaPlatform.Core.DTOs.Community;

namespace LetopiaPlatform.API.Validators;

public class UpdateCommunityRequestValidator : AbstractValidator<UpdateCommunityRequest>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public UpdateCommunityRequestValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(2).WithMessage("Community name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Community name cannot exceed 100 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MinimumLength(10).WithMessage("Description must be at least 10 characters.")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.CoverImage)
            .Must(file => file!.Length <= MaxFileSizeBytes)
                .WithMessage("Cover image must be at most 5 MB.")
            .Must(file => AllowedExtensions.Contains(Path.GetExtension(file!.FileName).ToLowerInvariant()))
                .WithMessage("Cover image must be a .jpg, .jpeg, .png, or .webp file.")
            .When(x => x.CoverImage is not null);
    }
}