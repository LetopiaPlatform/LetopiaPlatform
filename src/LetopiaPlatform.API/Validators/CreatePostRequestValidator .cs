using FluentValidation;
using LetopiaPlatform.Core.DTOs.Post;


namespace LetopiaPlatform.Core.Validators.Post;

public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    private const int MaxImages = 10;
    private const int MaxImageSizeMb = 5;
    private const int MaxTags = 10;

    public CreatePostRequestValidator()
    {
        // Title validation
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        // Content validation
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(10000).WithMessage("Content cannot exceed 10,000 characters.");

        // Images validation (optional)
        When(x => x.Images != null && x.Images.Count > 0, () =>
        {
            RuleFor(x => x.Images)
                .Must(images => images.Count <= MaxImages)
                .WithMessage($"You can upload up to {MaxImages} images only.");

            RuleForEach(x => x.Images)
                .Must(BeValidImage)
                .WithMessage($"Each image must be JPEG, PNG, WEBP, or GIF and <= {MaxImageSizeMb} MB.");
        });

        // Tags validation (optional)
        When(x => x.Tags != null && x.Tags.Count > 0, () =>
        {
            RuleFor(x => x.Tags)
                .Must(tags => tags.Count <= MaxTags)
                .WithMessage($"Maximum {MaxTags} tags allowed.");

            RuleForEach(x => x.Tags)
                .NotEmpty().WithMessage("Tags cannot be empty.")
                .MaximumLength(30).WithMessage("Tag cannot exceed 30 characters.")
                .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Tags can contain only letters, numbers, '-' and '_'.");
        });
    }

    // Helper to validate each image file
    private static bool BeValidImage(IFormFile file)
    {
        if (file == null)
            return false;

        var allowedTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif",
            "image/jfif"
        };

        var maxSize = MaxImageSizeMb * 1024 * 1024; // convert MB to bytes

        return allowedTypes.Contains(file.ContentType) && file.Length <= maxSize;
    }
}
