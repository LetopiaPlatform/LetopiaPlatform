using FluentValidation;
using LetopiaPlatform.Core.DTOs.CommunityResourse;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.API.Validators;

public class UpdateResourceRequestValidator : AbstractValidator<UpdateResourceRequest>
{
    public UpdateResourceRequestValidator()
    {
        // At least one field must be provided — sending an empty body is pointless
        RuleFor(x => x)
            .Must(HaveAtLeastOneField)
            .WithMessage("At least one field (Title, Description, Url, Type, or Tags) must be provided.");

        // ── Url (optional) ────────────────────────────────────────────────────

        RuleFor(x => x.Url)
            .MaximumLength(2048)
            .WithMessage("URL must not exceed 2048 characters.")
            .Must(BeAValidAbsoluteUrl)
            .WithMessage("URL must be a valid absolute URL (e.g. https://example.com).")
            .When(x => x.Url is not null);

        // ── Title (optional) ──────────────────────────────────────────────────

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title must not be empty when provided.")
            .MaximumLength(300)
            .WithMessage("Title must not exceed 300 characters.")
            .When(x => x.Title is not null);

        // ── Description (optional) ────────────────────────────────────────────

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description is not null);

        // ── Type (optional) ───────────────────────────────────────────────────

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage($"Type must be one of: {string.Join(", ", Enum.GetNames<ResourceType>())}.")
            .When(x => x.Type is not null);

        // ── Tags (optional) ───────────────────────────────────────────────────

        RuleFor(x => x.Tags)
            .Must(tags => tags!.Count <= 10)
            .WithMessage("A resource cannot have more than 10 tags.")
            .When(x => x.Tags is not null);

        RuleForEach(x => x.Tags)
            .NotEmpty()
            .WithMessage("Tag must not be empty.")
            .MaximumLength(100)
            .WithMessage("Each tag must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z0-9\-]+$")
            .WithMessage("Tags can only contain letters, numbers, and hyphens.")
            .When(x => x.Tags is not null);
    }

    private static bool HaveAtLeastOneField(UpdateResourceRequest x)
        => x.Title is not null
        || x.Description is not null
        || x.Url is not null
        || x.Type is not null
        || x.Tags is not null;

    private static bool BeAValidAbsoluteUrl(string? url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
