using FluentValidation;
using LetopiaPlatform.Core.DTOs.CommunityResourse;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.API.Validators;

public class CreateResourceRequestValidator : AbstractValidator<CreateResourceRequest>
{
    public CreateResourceRequestValidator()
    {
  



        // ── Url ───────────────────────────────────────────────────────────────

        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage("URL is required.")
            .MaximumLength(2048)
            .WithMessage("URL must not exceed 2048 characters.")
            .Must(BeAValidAbsoluteUrl)
            .WithMessage("URL must be a valid absolute URL (e.g. https://example.com).");

        // ── Type ──────────────────────────────────────────────────────────────

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage($"Type must be one of: {string.Join(", ", Enum.GetNames<ResourceType>())}.");

        // ── Title (optional) ──────────────────────────────────────────────────

        RuleFor(x => x.Title)
            .MaximumLength(300)
            .WithMessage("Title must not exceed 300 characters.")
            .When(x => x.Title is not null);

        // ── Description (optional) ────────────────────────────────────────────

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description is not null);

        // ── Tags (optional) ───────────────────────────────────────────────────

        RuleFor(x => x.Tags)
            .Must(tags => tags?.Count <= 10)
            .WithMessage("A resource cannot have more than 10 tags.")
            .When(x => x.Tags is not null);

        RuleForEach(x => x.Tags)
            .MaximumLength(100)
            .WithMessage("Each tag must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z0-9\-]+$")
            .WithMessage("Tags can only contain letters, numbers, and hyphens.");
    }

    private static bool BeAValidAbsoluteUrl(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
