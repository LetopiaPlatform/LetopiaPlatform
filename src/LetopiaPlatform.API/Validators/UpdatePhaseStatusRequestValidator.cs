using FluentValidation;
using LetopiaPlatform.Core.DTOs.Agent;

namespace LetopiaPlatform.API.Validators;

public class UpdatePhaseStatusRequestValidator : AbstractValidator<UpdatePhaseStatusRequest>
{
    public UpdatePhaseStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid phase status.");
    }
}
