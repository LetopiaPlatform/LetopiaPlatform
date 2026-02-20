namespace LetopiaPlatform.Core.DTOs.Category;

/// <summary>
/// Request DTO for creating a new category.
/// Slug is auto-generated from Name by the service layer.
/// </summary>
public sealed record CreateCategoryRequest(
    string Name,
    string Type,
    string? IconUrl = null,
    int DisplayOrder = 0);