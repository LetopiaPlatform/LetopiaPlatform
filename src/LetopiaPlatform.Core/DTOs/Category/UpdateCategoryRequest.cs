namespace LetopiaPlatform.Core.DTOs.Category;

/// <summary>
/// Request DTO for updating existing category.
/// </summary>
public sealed record UpdateCategoryRequest(
    string Name,
    string? IconUrl
);
