using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Exceptions;
using LetopiaPlatform.Core.Interfaces;

namespace LetopiaPlatform.Infrastructure.Services;

/// <summary>
/// Handles roadmap query, ownership validation, and phase updates.
/// </summary>
internal sealed class RoadmapService : IRoadmapService
{
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RoadmapService(IRoadmapRepository roadmapRepository, IUnitOfWork unitOfWork)
    {
        _roadmapRepository = roadmapRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<RoadmapSummaryDto>> ListRoadmapsForUserAsync(Guid userId, CancellationToken ct)
    {
        var roadmaps = await _roadmapRepository.GetByUserIdAsync(userId, ct);

        return roadmaps
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
    }

    public async Task<RoadmapDto> GetRoadmapAsync(Guid roadmapId, Guid userId, CancellationToken ct)
    {
        var roadmap = await _roadmapRepository.GetByIdWithPhasesAsync(roadmapId, ct)
            ?? throw new NotFoundException(nameof(Roadmap), roadmapId);

        if (roadmap.UserId != userId)
            throw new ForbiddenException();

        return new RoadmapDto(
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
    }

    public async Task<RoadmapPhaseDto> UpdatePhaseStatusAsync(
        Guid roadmapId, Guid phaseId, Guid userId, PhaseStatus status, CancellationToken ct)
    {
        // Targeted query: loads only the phase + its parent roadmap (not all sibling phases)
        var phase = await _roadmapRepository.GetPhaseByRoadmapAsync(phaseId, roadmapId, ct)
            ?? throw new NotFoundException(nameof(RoadmapPhase), phaseId);

        if (phase.Roadmap.UserId != userId)
            throw new ForbiddenException();

        phase.Status = status;

        // TODO: Gamification hook — award points when status transitions to Completed
        // if (status == PhaseStatus.Completed) { user.TotalPoints += X; }

        await _unitOfWork.SaveChangesAsync(ct);

        return new RoadmapPhaseDto(
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
    }
}
