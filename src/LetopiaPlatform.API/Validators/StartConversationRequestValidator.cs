using FluentValidation;
using LetopiaPlatform.Core.DTOs.Agent;

namespace LetopiaPlatform.API.Validators;

public class StartConversationRequestValidator : AbstractValidator<StartConversationRequest>
{
    public StartConversationRequestValidator()
    {
        RuleFor(x => x.InitialMessage)
            .NotEmpty().WithMessage("Initial message is required.")
            .MaximumLength(1000).WithMessage("Initial message must not exceed 1000 characters.");
    }
}
