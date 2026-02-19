using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.DTOs.ProjectCategory.Request;
public record CreateCategoryRequest(
    string Name,
    string Slug,
    IFormFile? IconUrl,
    int DisplayOrder
);
