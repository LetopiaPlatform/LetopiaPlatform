using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.DTOs.Category;

/// <summary>
/// Request DTO for creating a new category.
/// Slug is auto-generated from Name by the service layer.
/// </summary>
public sealed record CreateCategoryRequest(
    string Name,
    string Type,
    Guid? ParentCategoryId = null,
    IFormFile? Icon = null);