using System.Security.Claims;
using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.DTOs.CommunityTaskCategory.Request;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;
//[Route("api/[controller]")]
[ApiController]
public class CommunityTaskCategoryController : BaseController
{
    private readonly ICommunityTaskCategoryService _categoryService;

    public CommunityTaskCategoryController(ICommunityTaskCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    private Guid GetUserIdFromToken()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }

    // GET: api/communities/{communityId}/task-categories
    [HttpGet(Router.CommunityTaskCategory.GetAll)]
    [Authorize]
    public async Task<IActionResult> GetAll([FromRoute] Guid communityId)
    {
        var userId = GetUserIdFromToken();
        HttpContext.AddBusinessContext("action", "get_all_categories");
        HttpContext.AddBusinessContext("community_id", communityId.ToString());
        HttpContext.AddBusinessContext("user_id", userId.ToString());

        var result = await _categoryService.GetCommunityCategoriesAsync(communityId, userId, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpGet(Router.CommunityTaskCategory.GetCategoryById)]
    [Authorize]
    public async Task<IActionResult> GetById([FromRoute] Guid communityId, [FromRoute] Guid categoryid)
    {
        var userId = GetUserIdFromToken();
        HttpContext.AddBusinessContext("action", "get_category_by_id");
        HttpContext.AddBusinessContext("community_id", communityId.ToString());
        HttpContext.AddBusinessContext("category_id", categoryid.ToString());

        var result = await _categoryService.GetByIdAsync(communityId, categoryid, userId, HttpContext.RequestAborted);
        return HandleResult(result);
    }
    // POST: api/communities/{communityId}/task-categories
    [HttpPost(Router.CommunityTaskCategory.Create)]
    [Authorize]
    public async Task<IActionResult> Create([FromRoute] Guid communityId, [FromBody] CreateCategoryRequestDto request)
    {
        var userId = GetUserIdFromToken();
        HttpContext.AddBusinessContext("action", "create_category");
        HttpContext.AddBusinessContext("community_id", communityId.ToString());
        HttpContext.AddBusinessContext("owner_id", userId.ToString());

        var result = await _categoryService.CreateAsync(communityId, userId, request, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // PUT: api/communities/task-categories/{id}
    [HttpPut(Router.CommunityTaskCategory.Update)]
    [Authorize]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCategoryRequestDto request)
    {
        var userId = GetUserIdFromToken();
        HttpContext.AddBusinessContext("action", "update_category");
        HttpContext.AddBusinessContext("category_id", id.ToString());
        HttpContext.AddBusinessContext("owner_id", userId.ToString());

        var result = await _categoryService.UpdateAsync(id, userId, request, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // DELETE: api/communities/task-categories/{id}
    [HttpDelete(Router.CommunityTaskCategory.Delete)]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var userId = GetUserIdFromToken();
        HttpContext.AddBusinessContext("action", "delete_category");
        HttpContext.AddBusinessContext("category_id", id.ToString());
        HttpContext.AddBusinessContext("owner_id", userId.ToString());

        var result = await _categoryService.DeleteAsync(id, userId, HttpContext.RequestAborted);
        return HandleResult(result);
    }
}
