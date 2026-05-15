using FluentValidation;
using LetopiaPlatform.Core.DTOs.Project.Request;

namespace LetopiaPlatform.API.Validators;

public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequestDto>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Project title is required.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Project description is required.")
            .MinimumLength(15).WithMessage("Description must be at least 20 characters long.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");


        RuleForEach(x => x.Links)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(x => x.Links != null)
            .WithMessage("One or more links are not valid URLs.");

        RuleForEach(x => x.Files)
            .Must(file => file.Length <= 5 * 1024 * 1024)
            .When(x => x.Files != null)
            .WithMessage("Each resource file size must not exceed 5MB.");


    }

    private static bool IsSupportedImageType(string fileName)
    {
        var extensions = new[] { ".jpg", ".jpeg", ".png" };
        return extensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }
}
