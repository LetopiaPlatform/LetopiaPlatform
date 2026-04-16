using FluentValidation;
using LetopiaPlatform.Core.DTOs.Category;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.API.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MinimumLength(2).WithMessage("Category name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Category name must be at most 100 characters long.");
        
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Category type is required.")
            .Must(BeValidCategoryType).WithMessage($"Type must be {string.Join(", ", Enum.GetNames(typeof(CategoryType)))}.");
        
        RuleFor(x => x.Icon)
            .Must(BeValidSvgFile!)
            .WithMessage("Icon must be an SVG file under 256 KB.")
            .When(x => x.Icon is not null);
    }

    private static bool BeValidCategoryType(string type)
    {
        return Enum.TryParse<CategoryType>(type, ignoreCase: true, out _);
    }

    private static bool BeValidSvgFile(Microsoft.AspNetCore.Http.IFormFile file)
    {
        var extension = System.IO.Path.GetExtension(file.FileName);
        return string.Equals(extension, ".svg", StringComparison.OrdinalIgnoreCase)
            && file.Length <= 256 * 1024;
    }
}