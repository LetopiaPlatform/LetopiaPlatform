namespace LetopiaPlatform.Core.DTOs.ProjectCategory.Request;
public record UpdateCategoryRequest(
    string Name,
    string Slug,
    int DisplayOrder
);
