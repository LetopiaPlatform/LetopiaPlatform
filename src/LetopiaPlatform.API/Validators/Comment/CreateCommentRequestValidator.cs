using FluentValidation;
using LetopiaPlatform.Core.DTOs.Comment;

namespace LetopiaPlatform.API.Validators.comment;

public class CreateCommentRequestValidator
    : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
                .WithMessage("Content is required.")
            .MinimumLength(1)
            .MaximumLength(5000)
                .WithMessage("Content must be between 1 and 5000 characters.");
    }
}
