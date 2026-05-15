using FluentValidation;
using LetopiaPlatform.Core.DTOs.Agent;

namespace LetopiaPlatform.API.Validators;

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required.")
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters.");
    }
}
