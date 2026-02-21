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

        RuleFor(x => x.IconUrl)
            .Must(BeValidUrl).WithMessage("Icon URL must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.IconUrl));
    }

    private static bool BeValidUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}