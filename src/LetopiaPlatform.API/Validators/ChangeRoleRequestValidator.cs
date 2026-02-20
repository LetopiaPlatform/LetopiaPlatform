using FluentValidation;
using LetopiaPlatform.Core.DTOs.Community;

namespace LetopiaPlatform.API.Validators;

public class ChangeRoleRequestValidator : AbstractValidator<ChangeRoleRequest>
{
    private static readonly string[] ValidRoles = ["Member", "Moderator", "Owner"];

    public ChangeRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => ValidRoles.Contains(role))
            .WithMessage("Role must be one of: Member, Moderator, Owner.");
    }
}