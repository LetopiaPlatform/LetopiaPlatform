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
        
        RuleFor(x => x.IconUrl)
            .Must(BeValidUrl).WithMessage("Icon URL must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.IconUrl));
    }

    private static bool BeValidCategoryType(string type)
    {
        return Enum.TryParse<CategoryType>(type, ignoreCase: true, out _);
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}