
using Bokra.API.DTOs.File;
using FluentValidation;

namespace Bokra.API.Validators

{


    public class ReplaceFileDtoValidator : AbstractValidator<ReplaceFileDto>
    {
        public ReplaceFileDtoValidator()
        {
            RuleFor(x => x.NewFile)
                .NotNull().WithMessage("File is required")
                .Must(file => file != null && new[] { ".jpg", ".jpeg", ".png", ".pdf" }
                            .Contains(Path.GetExtension(file.FileName).ToLower()))
                .WithMessage("Only jpg, jpeg, png, or pdf files are allowed")
                .Must(file => file != null && file.Length <= 5 * 1024 * 1024)
                .WithMessage("File must be <= 5MB");

            RuleFor(x => x.Directory)
                .NotEmpty().WithMessage("Directory is required");

            RuleFor(x => x.OldFileUrl)
                .NotEmpty().WithMessage("Old file URL is required");
        }
    }

}
