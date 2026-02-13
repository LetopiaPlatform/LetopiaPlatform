using FluentValidation;
using LetopiaPlatform.Core.Community;

namespace LetopiaPlatform.API.Validators.Community;

public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Group name is required.")
            .MinimumLength(2).WithMessage("Group name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Group name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Group description cannot exceed 500 characters.")
            .When(x => x.Description is not null);
    }
}