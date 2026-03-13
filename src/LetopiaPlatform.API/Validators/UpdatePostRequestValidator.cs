using FluentValidation;
using LetopiaPlatform.Core.DTOs.Post;
using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.Validators.Post;

public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    private const int MaxImages = 10;
    private const int MaxImageSizeMb = 5;
    private const int MaxTags = 10;

    public UpdatePostRequestValidator()
    {
        // Title validation (optional)
        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");
        });

        // Content validation (optional)
        When(x => x.Content != null, () =>
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content cannot be empty.")
                .MaximumLength(10000).WithMessage("Content cannot exceed 10,000 characters.");
        });

        // AddImages validation (optional)
        When(x => x.AddImages != null && x.AddImages.Count > 0, () =>
        {
            RuleFor(x => x.AddImages)
                .Must(images => images.Count <= MaxImages)
                .WithMessage($"You can upload up to {MaxImages} images only.");

            RuleForEach(x => x.AddImages)
                .Must(BeValidImage)
                .WithMessage($"Each image must be JPEG, PNG, WEBP, or GIF and <= {MaxImageSizeMb} MB.");
        });

        // RemoveImageUrls validation (optional)
        When(x => x.RemoveImageUrls != null && x.RemoveImageUrls.Count > 0, () =>
        {
            RuleForEach(x => x.RemoveImageUrls)
                .NotEmpty().WithMessage("RemoveImageUrls cannot contain empty URLs.")
                .MaximumLength(500).WithMessage("URL is too long."); // optional limit
        });

        // Tags validation (optional)
        When(x => x.Tags != null, () =>
        {
            

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
            "image/gif"
        };

        var maxSize = MaxImageSizeMb * 1024 * 1024; // convert MB to bytes

        return allowedTypes.Contains(file.ContentType) && file.Length <= maxSize;
    }
}
