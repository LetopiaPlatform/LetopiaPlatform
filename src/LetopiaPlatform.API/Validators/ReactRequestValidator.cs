using FluentValidation;

using LetopiaPlatform.Core.DTOs.Reaction;

namespace LetopiaPlatform.API.Validators;

public class ReactRequestValidator : AbstractValidator<ReactRequestDto>
{
    public ReactRequestValidator()
    {
        RuleFor(x => x.ReactionType)
            .NotNull().WithMessage("ReactionType is required.")
            .IsInEnum().WithMessage("ReactionType must be a valid value: Upvote or Downvote.");
    }
}
