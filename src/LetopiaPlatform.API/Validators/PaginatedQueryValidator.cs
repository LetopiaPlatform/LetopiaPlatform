using FluentValidation;
using LetopiaPlatform.Core.Common;

namespace LetopiaPlatform.API.Validators;

public class PaginatedQueryValidator : AbstractValidator<PaginatedQuery>
{
    public PaginatedQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("Page size must be between 1 and 50.");
    }
}