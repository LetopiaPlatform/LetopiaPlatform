using FluentValidation;
using LetopiaPlatform.Core.DTOs.Project.Request;

namespace LetopiaPlatform.API.Validators;

public class CreateMilestoneDtoValidator : AbstractValidator<MilestoneRequestDto>
{
    public CreateMilestoneDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Milestone title is required.")
            .MaximumLength(150).WithMessage("Milestone title cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Milestone description cannot exceed 500 characters.");
    }
}
