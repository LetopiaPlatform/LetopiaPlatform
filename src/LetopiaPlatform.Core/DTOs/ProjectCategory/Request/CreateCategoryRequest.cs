namespace LetopiaPlatform.Core.DTOs.ProjectCategory.Request;
public record CreateCategoryRequest(
    string Name,
    string Slug,
    string? IconUrl,
    int DisplayOrder
);
