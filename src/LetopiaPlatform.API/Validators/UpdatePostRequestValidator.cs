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
        RuleFor(x => x.PostImage)
              .Must(file => file == null || IsValidImage(file))
              .WithMessage("Only JPG, PNG, and WEBP images are allowed.")
              .Must(file => file == null || file.Length <= 5 * 1024 * 1024)
              .WithMessage("Image size must not exceed 5MB.");

    }
    private static bool IsValidImage(IFormFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        return allowedTypes.Contains(file.ContentType);
    }
}
