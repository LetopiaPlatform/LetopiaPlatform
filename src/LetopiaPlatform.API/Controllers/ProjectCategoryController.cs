using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;
//[Route("api/[controller]")]
[ApiController]
public class ProjectCategoryController : BaseController
{
    private readonly IProjectCategoryService _projectCategoryService;

    public ProjectCategoryController(IProjectCategoryService projectCategoryService)
    {
        _projectCategoryService = projectCategoryService;
    }

    // ── Get All Categories ───────────────────────────────────────────────────
    /// <summary>
    /// جلب جميع الأقسام مرتبة مع المشاريع المتاحة داخل كل قسم
    /// </summary>
    [HttpGet(Router.ProjectCategories.GetCategories)]
    [AllowAnonymous] // متاح للزوار
    public async Task<IActionResult> GetCategories()
    {
        HttpContext.AddBusinessContext("action", "get_all_categories_with_projects");

        var result = await _projectCategoryService.GetAllOrderedAsync(HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // ── Get Category By Slug ─────────────────────────────────────────────────
    /// <summary>
    /// جلب تفاصيل قسم معين عن طريق الـ Slug
    /// </summary>
    [HttpGet(Router.ProjectCategories.GetCategoryBySlug)]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoryBySlug([FromRoute] string slug)
    {
        HttpContext.AddBusinessContext("action", "get_category_by_slug");
        HttpContext.AddBusinessContext("slug", slug);

        var result = await _projectCategoryService.GetBySlugAsync(slug, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // ── Get Statistics ──────────────────────────────────────────────────────
    /// <summary>
    /// جلب إحصائيات عدد المشاريع في كل قسم
    /// </summary>
    [HttpGet(Router.ProjectCategories.GetCategoryStats)]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoryStats()
    {
        HttpContext.AddBusinessContext("action", "get_category_stats");

        var result = await _projectCategoryService.GetCategoryStatsAsync(HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // ── Delete Category ──────────────────────────────────────────────────────
    /// <summary>
    /// حذف قسم (للأدمن فقط وبشرط عدم وجود مشاريع مرتبطة)
    /// </summary>
    [HttpDelete(Router.ProjectCategories.DeleteCategory)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory([FromRoute] Guid id)
    {
        HttpContext.AddBusinessContext("action", "delete_category");
        HttpContext.AddBusinessContext("category_id", id.ToString());

        var result = await _projectCategoryService.DeleteCategoryAsync(id, HttpContext.RequestAborted);
        return HandleResult(result);
    }
}
