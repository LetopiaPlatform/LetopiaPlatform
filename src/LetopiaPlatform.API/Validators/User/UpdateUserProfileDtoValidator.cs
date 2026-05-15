using FluentValidation;
using LetopiaPlatform.Core.DTOs.User;

namespace LetopiaPlatform.API.Validators.User;

public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateUserProfileDtoValidator()
    {
        // ── Full Name ─────────────────────────────
        RuleFor(x => x.FullName)
            .MaximumLength(50)
            .WithMessage("Name must be at most 50 characters.");

        // ── Bio ───────────────────────────────────
        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .WithMessage("Bio must be at most 500 characters.");

        // ── Phone ─────────────────────────────────
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?\d{7,15}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Invalid phone number format.");

        // ── Location ──────────────────────────────
        RuleFor(x => x.Location)
            .MaximumLength(100)
            .WithMessage("Location must be at most 100 characters.");

        // ── Social Links ──────────────────────────
        RuleFor(x => x.SocialLinks)
            .Must(links =>
            {
                if (links == null) return true;
                var duplicates = links
                    .GroupBy(x => x.Provider, StringComparer.OrdinalIgnoreCase)
                    .Any(g => g.Count() > 1);
                return !duplicates;
            })
            .WithMessage("Duplicate social link providers are not allowed.");

        RuleForEach(x => x.SocialLinks)
            .ChildRules(link =>
            {
                link.RuleFor(x => x.Provider)
                    .NotEmpty()
                    .MaximumLength(50);

                link.RuleFor(x => x.Url)
                    .NotEmpty()
                    .Must(BeAValidUrl)
                    .WithMessage("Invalid URL format.");
            });

        // ── Skills ────────────────────────────────
        RuleFor(x => x.Skills)
            .Must(skills => skills == null || skills.Count <= 20)
            .WithMessage("You cannot add more than 20 skills.")
            .Must(skills =>
            {
                if (skills == null) return true;
                return skills.Count == skills.Distinct(StringComparer.OrdinalIgnoreCase).Count();
            })
            .WithMessage("Duplicate skills are not allowed.");

        RuleForEach(x => x.Skills)
            .NotEmpty()
            .WithMessage("Skill cannot be empty.")
            .MaximumLength(50)
            .WithMessage("Each skill must be at most 50 characters.")
            .Matches(@"^[a-zA-Z0-9 .+#\-]+$")
            .WithMessage("Skill contains invalid characters.");

        // ── Interests ─────────────────────────────
        RuleFor(x => x.Interests)
            .Must(interests => interests == null || interests.Count <= 20)
            .WithMessage("You cannot add more than 20 interests.")
            .Must(interests =>
            {
                if (interests == null) return true;
                return interests.Count == interests.Distinct(StringComparer.OrdinalIgnoreCase).Count();
            })
            .WithMessage("Duplicate interests are not allowed.");

        RuleForEach(x => x.Interests)
            .NotEmpty()
            .WithMessage("Interest cannot be empty.")
            .MaximumLength(50)
            .WithMessage("Each interest must be at most 50 characters.")
            .Matches(@"^[a-zA-Z0-9 .+#\-]+$")
            .WithMessage("Interest contains invalid characters.");
    }

    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
