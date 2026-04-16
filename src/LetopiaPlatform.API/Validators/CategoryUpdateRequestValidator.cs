using FluentValidation;
using LetopiaPlatform.Core.DTOs.Category;

namespace LetopiaPlatform.API.Validators;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MinimumLength(2).WithMessage("Category name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Category name must be at most 100 characters.");

        RuleFor(x => x.Icon)
            .Must(BeValidSvgFile!)
            .WithMessage("Icon must be an SVG file under 256 KB.")
            .When(x => x.Icon is not null);
    }

    private static bool BeValidSvgFile(IFormFile file)
    {
        var extension = System.IO.Path.GetExtension(file.FileName);
        return string.Equals(extension, ".svg", StringComparison.OrdinalIgnoreCase)
            && file.Length <= 256 * 1024;
    }
}