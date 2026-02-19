using FluentValidation;
using LetopiaPlatform.Core.DTOs.Post;

namespace LetopiaPlatform.API.Validators;

public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .Length(5, 200).WithMessage("Title must be between 5 and 200 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters.");

        RuleFor(x => x.PostType)
            .IsInEnum().WithMessage("PostType must be a valid value: Discussion, Question, Resource.");
    }
}
