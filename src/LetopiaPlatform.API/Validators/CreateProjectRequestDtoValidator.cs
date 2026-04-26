using FluentValidation;
using LetopiaPlatform.Core.DTOs.Project.Request;

namespace LetopiaPlatform.API.Validators;

public class CreateProjectRequestDtoValidator : AbstractValidator<CreateProjectRequestDto>
{
    public CreateProjectRequestDtoValidator()
    {
        // 1. Basic Information
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Project title is required.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Project description is required.")
            .MinimumLength(20).WithMessage("Description must be at least 20 characters long.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Please select a project category.");

        // 2. Timeline and Dates (Preventing logic errors before Database)
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Start date cannot be in the past.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after the start date.");




        // 4. Milestones (Validating the nested list)
        RuleForEach(x => x.Milestones).SetValidator(new CreateMilestoneDtoValidator());

        // 5. Cover Image Validation (Size and Type)
        RuleFor(x => x.CoverImage)
       .Must(file => file == null || file.Length <= 2 * 1024 * 1024)
       .WithMessage("Image size must not exceed 2MB.")
       .Must(file => file == null || IsSupportedFileType(file.FileName))
        .WithMessage("File type not supported (Only JPG, PNG, and JPEG are allowed).");
    }

    private static bool IsSupportedFileType(string fileName)
    {
        var extensions = new[] { ".jpg", ".jpeg", ".png" };

        return extensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }
}
