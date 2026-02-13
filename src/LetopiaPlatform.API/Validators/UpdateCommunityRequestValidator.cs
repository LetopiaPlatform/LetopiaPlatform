using FluentValidation;
using LetopiaPlatform.Core.Community;

namespace LetopiaPlatform.API.Validators;

public class UpdateCommunityRequestValidator : AbstractValidator<UpdateCommunityRequest>
{
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
        
        RuleFor(x => x.IconUrl)
            .Must(BeValidUrl).WithMessage("Icon URL must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.IconUrl));
        
        RuleFor(x => x.CoverImageUrl)
            .Must(BeValidUrl).WithMessage("Cover image URL must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.CoverImageUrl));
        
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}