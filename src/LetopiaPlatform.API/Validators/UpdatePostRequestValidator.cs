using FluentValidation;
using LetopiaPlatform.Core.DTOs.Post;

namespace LetopiaPlatform.API.Validators;

public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        // Only validate if Title is provided
        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title)
                .Length(5, 200).WithMessage("Title must be between 5 and 200 characters.");
        });

        // Only validate if Content is provided
        When(x => x.Content != null, () =>
        {
            RuleFor(x => x.Content)
                .MinimumLength(10).WithMessage("Content must be at least 10 characters.");
        });
    }
}
