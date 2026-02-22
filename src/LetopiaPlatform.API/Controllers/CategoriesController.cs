using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Common;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.DTOs.Category;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;

[ApiController]
public class CategoriesController : BaseController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet(Router.Categories.GetByType)]
    [AllowAnonymous]
    public async Task<IActionResult> GetByType(string type, CancellationToken ct)
    {
        HttpContext.AddBusinessContext("action", "get_categories_by_type");

        var categories = await _categoryService.GetByTypeAsync(type, ct);
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(categories));
    }

    [HttpGet(Router.Categories.GetBySlug)]
    [AllowAnonymous]
    public async Task<IActionResult> GetBySlug(string slug, string type, CancellationToken ct)
    {
        HttpContext.AddBusinessContext("action", "get_category_by_slug");

        var category = await _categoryService.GetBySlugAsync(slug, type, ct);
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(category));
    }
    
    [HttpPost(Router.Categories.Prefix)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        HttpContext.AddBusinessContext("action", "create_category");

        var category = await _categoryService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CategoryDto>.SuccessResponse(category, "Category created successfully", 201));
    }

    [HttpPut(Router.Categories.Update)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request, CancellationToken ct)
    {
        HttpContext.AddBusinessContext("action", "update_category");
        HttpContext.AddBusinessContext("category_id", id.ToString());

        var category = await _categoryService.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(category, "Category updated successfully"));
    }

    [HttpDelete(Router.Categories.Delete)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        HttpContext.AddBusinessContext("action", "delete_category");
        HttpContext.AddBusinessContext("category_id", id.ToString());

        await _categoryService.DeleteAsync(id, ct);
        return NoContent();
    }
}