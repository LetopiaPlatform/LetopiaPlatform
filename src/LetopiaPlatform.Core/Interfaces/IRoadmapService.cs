using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Service contract for roadmap query, ownership, and phase update operations.
/// </summary>
public interface IRoadmapService
{
    /// <summary>
    /// Retrieves all roadmaps for the specified user, ordered by most recently created.
    /// </summary>
    Task<List<RoadmapSummaryDto>> ListRoadmapsForUserAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// Retrieves a full roadmap (including phases) after verifying ownership.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the roadmap does not exist.</exception>
    /// <exception cref="Exceptions.ForbiddenException">Thrown when the roadmap belongs to another user.</exception>
    Task<RoadmapDto> GetRoadmapAsync(Guid roadmapId, Guid userId, CancellationToken ct);

    /// <summary>
    /// Updates the status of a single phase after verifying roadmap ownership.
    /// Uses a targeted query — loads only the phase, not all sibling phases.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the roadmap or phase does not exist.</exception>
    /// <exception cref="Exceptions.ForbiddenException">Thrown when the roadmap belongs to another user.</exception>
    Task<RoadmapPhaseDto> UpdatePhaseStatusAsync(
        Guid roadmapId, Guid phaseId, Guid userId, PhaseStatus status, CancellationToken ct);
}
