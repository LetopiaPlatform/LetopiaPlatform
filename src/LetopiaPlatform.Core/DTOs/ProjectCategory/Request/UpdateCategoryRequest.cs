using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.DTOs.ProjectCategory.Request;
public record UpdateCategoryRequest(
    string Name,
    string Slug,
    IFormFile? IconUrl,
    int DisplayOrder
);
