using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Common;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;

[Authorize]
public class RoadmapsController : BaseController
{
    private readonly IRoadmapService _roadmapService;

    public RoadmapsController(IRoadmapService roadmapService)
    {
        _roadmapService = roadmapService;
    }

    /// <summary>
    /// List all roadmaps for the authenticated user.
    /// </summary>
    [HttpGet(Router.Roadmaps.List)]
    [ProducesResponseType(typeof(ApiResponse<List<RoadmapSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListRoadmaps(CancellationToken ct)
    {
        var userId = GetUserId();

        HttpContext.AddBusinessContext("action", "list_roadmaps");

        var dtos = await _roadmapService.ListRoadmapsForUserAsync(userId, ct);

        return Ok(ApiResponse<List<RoadmapSummaryDto>>.SuccessResponse(dtos));
    }

    /// <summary>
    /// Get a roadmap with all its phases.
    /// </summary>
    [HttpGet(Router.Roadmaps.GetById)]
    [ProducesResponseType(typeof(ApiResponse<RoadmapDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoadmap(Guid roadmapId, CancellationToken ct)
    {
        var userId = GetUserId();

        HttpContext.AddBusinessContext("action", "get_roadmap");
        HttpContext.AddBusinessContext("roadmap_id", roadmapId);

        // Throws NotFoundException / ForbiddenException — handled by ExceptionMiddleware
        var dto = await _roadmapService.GetRoadmapAsync(roadmapId, userId, ct);

        return Ok(ApiResponse<RoadmapDto>.SuccessResponse(dto));
    }

    /// <summary>
    /// Update the status of a roadmap phase.
    /// This is the gamification hook — future tasks can add point awards on Completed status.
    /// </summary>
    [HttpPatch(Router.Roadmaps.UpdatePhaseStatus)]
    [ProducesResponseType(typeof(ApiResponse<RoadmapPhaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePhaseStatus(
        Guid roadmapId,
        Guid phaseId,
        [FromBody] UpdatePhaseStatusRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();

        HttpContext.AddBusinessContext("action", "update_phase_status");
        HttpContext.AddBusinessContext("roadmap_id", roadmapId);
        HttpContext.AddBusinessContext("phase_id", phaseId);

        // Throws NotFoundException / ForbiddenException — handled by ExceptionMiddleware
        var dto = await _roadmapService.UpdatePhaseStatusAsync(roadmapId, phaseId, userId, request.Status, ct);

        return Ok(ApiResponse<RoadmapPhaseDto>.SuccessResponse(dto));
    }
}
