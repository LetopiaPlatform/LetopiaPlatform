using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Agent;

/// <summary>
/// Lightweight roadmap representation for list views.
/// </summary>
public sealed record RoadmapSummaryDto(
    Guid Id,
    string Title,
    string Topic,
    RoadmapStatus Status,
    int PhaseCount,
    int CompletedPhaseCount,
    DateTime CreatedAt);
