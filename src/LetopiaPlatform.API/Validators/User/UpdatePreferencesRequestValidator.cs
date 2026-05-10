using FluentValidation;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.User;

namespace LetopiaPlatform.API.Validators.User;

public class UpdatePreferencesRequestValidator : AbstractValidator<UpdatePreferencesRequest>
{
    public UpdatePreferencesRequestValidator()
    {
        RuleFor(x => x.PrivacySettings)
            .SetValidator(new PrivacySettingsValidator() as IValidator<PrivacySettings?>)
            .When(x => x.PrivacySettings != null);

    }
}

public class PrivacySettingsValidator : AbstractValidator<PrivacySettings>
{
    public PrivacySettingsValidator()
    {
        RuleFor(x => x.ProfileVisibility)
            .IsInEnum()
            .WithMessage("Invalid profile visibility value.");
    }
}
