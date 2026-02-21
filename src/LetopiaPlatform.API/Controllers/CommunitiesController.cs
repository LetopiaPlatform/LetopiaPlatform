using System.Security.Claims;
using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Common;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Community;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;

[Authorize]
public class CommunitiesController : BaseController
{
    private readonly ICommunityService _communityService;

    public CommunitiesController(ICommunityService communityService)
    {
        _communityService = communityService;
    }

    /// <summary>
    /// Create a new community. Caller becomes the Owner.
    /// Two default channels (Announcements + General) are auto-created.
    /// </summary>
    [HttpPost(Router.Communities.Create)]
    [ProducesResponseType(typeof(ApiResponse<CommunityDetailDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromForm] CreateCommunityRequest request,
        CancellationToken ct)
    {
        HttpContext.AddBusinessContext("action", "create_community");
        HttpContext.AddBusinessContext("community_name", request.Name);

        var result = await _communityService.CreateAsync(request, GetUserId(User), ct);

        HttpContext.AddBusinessContext("community_id", result.Id);
        HttpContext.AddBusinessContext("community_slug", result.Slug);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CommunityDetailDto>.SuccessResponse(result, "Community created successfully", 201));
    }

    /// <summary>
    /// List communities with optional filtering, searching, and sorting.
    /// </summary>
    [HttpGet(Router.Communities.List)]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<CommunitySummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] PaginatedQuery query,
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        CancellationToken ct)
    {
        HttpContext.AddBusinessContext("action", "list_communities");

        Guid? currentUserId = User.Identity?.IsAuthenticated == true ? GetUserId(User) : null;

        var result = await _communityService.ListAsync(query, category, search, sortBy, currentUserId, ct);

        return Ok(ApiResponse<PaginatedResult<CommunitySummaryDto>>
            .SuccessResponse(result));
    }

    /// <summary>
    /// List all communities the current user has joined.
    /// </summary>
    [HttpGet(Router.Communities.MyCommunities)]
    [ProducesResponseType(typeof(ApiResponse<List<JoinedCommunitySummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MyCommunities(CancellationToken ct)
    {
        HttpContext.AddBusinessContext("action", "get_my_communities");

        var result = await _communityService.GetJoinedCommunitiesAsync(GetUserId(User), ct);

        return Ok(ApiResponse<List<JoinedCommunitySummaryDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get full community details by slug, including channels and membership context.
    /// </summary>
    [HttpGet(Router.Communities.GetBySlug)]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<CommunityDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySlug(
        string slug,
        CancellationToken ct)
    {
        HttpContext.AddBusinessContext("action", "get_community");
        HttpContext.AddBusinessContext("community_slug", slug);

        Guid? currentUserId = User.Identity?.IsAuthenticated == true ? GetUserId(User) : null;

        var result = await _communityService.GetBySlugAsync(slug, currentUserId, ct);

        return Ok(ApiResponse<CommunityDetailDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Update community settings. Only Owner or Moderator can update.
    /// </summary>
    [HttpPut(Router.Communities.Update)]
    [ProducesResponseType(typeof(ApiResponse<CommunityDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromForm] UpdateCommunityRequest request,
        CancellationToken ct)
    {
        HttpContext.AddBusinessContext("action", "update_community");
        HttpContext.AddBusinessContext("community_id", id);

        var result = await _communityService.UpdateAsync(id, request, GetUserId(User), ct);

        return Ok(ApiResponse<CommunityDetailDto>.SuccessResponse(result, "Community updated successfully"));
    }

    /// <summary>
    /// Join a community as a Member.
    /// </summary>
    [HttpPost(Router.Communities.Join)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Join(Guid id, CancellationToken ct)
    {
        HttpContext.AddBusinessContext("action", "join_community");
        HttpContext.AddBusinessContext("community_id", id);

        await _communityService.JoinAsync(id, GetUserId(User), ct);

        return NoContent();
    }

    /// <summary>
    /// Leave a community. Owner cannot leave (must transfer ownership first).
    /// </summary>
    [HttpDelete(Router.Communities.Leave)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Leave(Guid id, CancellationToken ct)
    {
        HttpContext.AddBusinessContext("action", "leave_community");
        HttpContext.AddBusinessContext("community_id", id);

        await _communityService.LeaveAsync(id, GetUserId(User), ct);

        return NoContent();
    }

    /// <summary>
    /// List community members with pagination.
    /// </summary>
    [HttpGet(Router.Communities.Members)]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<MemberDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers(
        Guid id,
        [FromQuery] PaginatedQuery query,
        CancellationToken ct)
    {
        HttpContext.AddBusinessContext("action", "get_members");
        HttpContext.AddBusinessContext("community_id", id);

        var result = await _communityService.GetMembersAsync(id, query, ct);

        return Ok(ApiResponse<PaginatedResult<MemberDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Change a member's role. Only the Owner can do this.
    /// Transferring to Owner is atomic: new owner gets Owner, old owner becomes Moderator.
    /// </summary>
    [HttpPut(Router.Communities.ChangeRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeRole(
        Guid id,
        Guid userId,
        [FromBody] ChangeRoleRequest request,
        CancellationToken ct)
    {
        HttpContext.AddBusinessContext("action", "change_role");
        HttpContext.AddBusinessContext("community_id", id);
        HttpContext.AddBusinessContext("target_user_id", userId);
        HttpContext.AddBusinessContext("new_role", request.Role);

        await _communityService.ChangeRoleAsync(id, userId, request, GetUserId(User), ct);

        return NoContent();
    }

    private static Guid GetUserId(ClaimsPrincipal claimsPrincipal)
    {
        return Guid.Parse(claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }
}