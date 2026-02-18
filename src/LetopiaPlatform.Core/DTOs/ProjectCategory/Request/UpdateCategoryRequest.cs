namespace LetopiaPlatform.Core.DTOs.ProjectCategory.Request;
public record UpdateCategoryRequest(
    string Name,
    string Slug,
    string? IconUrl,
    int DisplayOrder
);
