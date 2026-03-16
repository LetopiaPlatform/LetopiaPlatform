using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.DTOs.CommunityResourse;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;

[ApiController]
[Authorize]
public class ResourceController : BaseController
{
    private readonly IResourceService _resourceService;

    public ResourceController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    // POST: api/communities/{communityId}/resources
    [HttpPost(Router.Resources.Create)]
 
    public async Task<IActionResult> Create(
        [FromRoute] Guid communityId,
        [FromBody] CreateResourceRequest request)
    {


        var userId = GetUserId();
        HttpContext.AddBusinessContext("action", "create_resource");
        HttpContext.AddBusinessContext("community_id", communityId.ToString());
        HttpContext.AddBusinessContext("user_id", userId.ToString());

        var result = await _resourceService.CreateResourceAsync(request, communityId, userId, HttpContext.RequestAborted);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetResource), new { communityId, resourceId = result.Value!.Id }, result);

        return HandleResult(result);
    }

    // PUT: api/communities/{communityId}/resources/{resourceId}
    [HttpPut(Router.Resources.Update)]
 
    public async Task<IActionResult> Update(

        [FromRoute] Guid resourceId,
        [FromBody] UpdateResourceRequest request)
    {
        var userId = GetUserId();
        HttpContext.AddBusinessContext("action", "update_resource");
        HttpContext.AddBusinessContext("resource_id", resourceId.ToString());
        HttpContext.AddBusinessContext("user_id", userId.ToString());

        var result = await _resourceService.UpdateResourceAsync(resourceId, request, userId, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // GET: api/communities/{communityId}/resources
    [HttpGet(Router.Resources.List)]
 
    public async Task<IActionResult> GetAll(
        [FromRoute] Guid communityId,
        [FromQuery] ResourceQueryParams query)
    {
        var userId = GetUserId();
        HttpContext.AddBusinessContext("action", "get_community_resources");
        HttpContext.AddBusinessContext("community_id", communityId.ToString());

        var result = await _resourceService.GetResourcesAsync(communityId, query, userId, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // GET: api/communities/{communityId}/resources/{resourceId}
    [HttpGet(Router.Resources.GetById)]
 
    public async Task<IActionResult> GetResource(
        [FromRoute] Guid communityId,
        [FromRoute] Guid resourceId)
    {
        var userId = GetUserId();
        HttpContext.AddBusinessContext("action", "get_resource");
        HttpContext.AddBusinessContext("resource_id", resourceId.ToString());

        var result = await _resourceService.GetResourceAsync(resourceId, userId, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // GET: api/communities/{communityId}/resources/recommended
    [HttpGet(Router.Resources.Recommended)]
 
    public async Task<IActionResult> GetRecommended(
        [FromRoute] Guid communityId,
   
        [FromQuery] ResourceQueryParams query)
    {
        var userId = GetUserId();
        HttpContext.AddBusinessContext("action", "get_recommended_resources");
        HttpContext.AddBusinessContext("community_id", communityId.ToString());

        var result = await _resourceService.GetRecommendedAsync(communityId,query.Type??ResourceType.Video ,query, userId, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // POST: api/communities/{communityId}/resources/{resourceId}/view
    [HttpPost(Router.Resources.AddView)]
 
    public async Task<IActionResult> AddView(
      
        [FromRoute] Guid resourceId)
    {
        HttpContext.AddBusinessContext("action", "add_resource_view");
        HttpContext.AddBusinessContext("resource_id", resourceId.ToString());

        var result = await _resourceService.AddViewAsync(resourceId, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // POST: api/communities/{communityId}/resources/{resourceId}/like
    [HttpPost(Router.Resources.ToggleLike)]
 
    public async Task<IActionResult> ToggleLike(
      
        [FromRoute] Guid resourceId)
    {
        var userId = GetUserId();
        HttpContext.AddBusinessContext("action", "toggle_resource_like");
        HttpContext.AddBusinessContext("resource_id", resourceId.ToString());
        HttpContext.AddBusinessContext("user_id", userId.ToString());

        var result = await _resourceService.ToggleLikeAsync(resourceId, userId, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // DELETE: api/communities/{communityId}/resources/{resourceId}
    [HttpDelete(Router.Resources.Delete)]
 
    public async Task<IActionResult> Delete(
    
        [FromRoute] Guid resourceId)
    {
        var userId = GetUserId();
        HttpContext.AddBusinessContext("action", "delete_resource");
        HttpContext.AddBusinessContext("resource_id", resourceId.ToString());
        HttpContext.AddBusinessContext("user_id", userId.ToString());

        var result = await _resourceService.DeleteResourceAsync(resourceId, userId, HttpContext.RequestAborted);
        return HandleResult(result);
    }
}
