using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Common;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;

[Authorize]
public class RoadmapsController : BaseController
{
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;

    public RoadmapsController(
        IRoadmapRepository roadmapRepository,
        IUnitOfWork<ApplicationDbContext> unitOfWork)
    {
        _roadmapRepository = roadmapRepository;
        _unitOfWork = unitOfWork;
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

        var roadmaps = await _roadmapRepository.GetByUserIdAsync(userId, ct);

        var dtos = roadmaps
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RoadmapSummaryDto(
                r.Id,
                r.Title,
                r.Topic,
                r.Status,
                r.Phases.Count,
                r.Phases.Count(p => p.Status == PhaseStatus.Completed),
                r.CreatedAt))
            .ToList();

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

        var roadmap = await _roadmapRepository.GetByIdWithPhasesAsync(roadmapId, ct);

        if (roadmap is null)
            return NotFound(new ErrorResponse { Status = 404, Message = "Roadmap not found." });

        if (roadmap.UserId != userId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new ErrorResponse { Status = 403, Message = "You do not have access to this roadmap." });

        var dto = new RoadmapDto(
            roadmap.Id,
            roadmap.UserId,
            roadmap.ConversationId,
            roadmap.Title,
            roadmap.Topic,
            roadmap.Description,
            roadmap.Status,
            roadmap.EstimatedDurationWeeks,
            roadmap.CreatedAt,
            roadmap.UpdatedAt,
            roadmap.Phases
                .OrderBy(p => p.Order)
                .Select(p => new RoadmapPhaseDto(
                    p.Id,
                    p.Title,
                    p.Description,
                    p.Order,
                    p.Status,
                    p.DurationEstimateWeeks,
                    p.Resources,
                    p.Projects,
                    p.Insights,
                    p.CreatedAt,
                    p.UpdatedAt))
                .ToList());

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

        // Validate roadmap exists and belongs to user
        var roadmap = await _roadmapRepository.GetByIdWithPhasesAsync(roadmapId, ct);

        if (roadmap is null)
            return NotFound(new ErrorResponse { Status = 404, Message = "Roadmap not found." });

        if (roadmap.UserId != userId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new ErrorResponse { Status = 403, Message = "You do not have access to this roadmap." });

        // Validate phase exists and belongs to this roadmap
        var phase = roadmap.Phases.FirstOrDefault(p => p.Id == phaseId);

        if (phase is null)
            return NotFound(new ErrorResponse { Status = 404, Message = "Phase not found." });

        // Update the phase status
        phase.Status = request.Status;

        // TODO: Gamification hook — award points when status transitions to Completed
        // if (request.Status == PhaseStatus.Completed) { user.TotalPoints += X; }

        await _unitOfWork.SaveChangesAsync(ct);

        var dto = new RoadmapPhaseDto(
            phase.Id,
            phase.Title,
            phase.Description,
            phase.Order,
            phase.Status,
            phase.DurationEstimateWeeks,
            phase.Resources,
            phase.Projects,
            phase.Insights,
            phase.CreatedAt,
            phase.UpdatedAt);

        return Ok(ApiResponse<RoadmapPhaseDto>.SuccessResponse(dto));
    }
}
