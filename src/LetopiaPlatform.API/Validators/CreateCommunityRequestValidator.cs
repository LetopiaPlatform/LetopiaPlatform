using FluentValidation;
using LetopiaPlatform.Core.DTOs.Community;

namespace LetopiaPlatform.API.Validators;

public class CreateCommunityRequestValidator : AbstractValidator<CreateCommunityRequest>
{
    public CreateCommunityRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Community name is required.")
            .MinimumLength(2).WithMessage("Community name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Community name must be at most 100 characters.");
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters.")
            .MaximumLength(2000).WithMessage("Description must be at most 2000 characters.");
        
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.IconUrl)
            .Must(BeValidUrl).WithMessage("Icon URL must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.IconUrl));
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}