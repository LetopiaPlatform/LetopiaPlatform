using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Agent;

/// <summary>
/// Full roadmap representation for graph display, including all phases.
/// </summary>
public sealed record RoadmapDto(
    Guid Id,
    Guid UserId,
    Guid ConversationId,
    string Title,
    string Topic,
    string Description,
    RoadmapStatus Status,
    int EstimatedDurationWeeks,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<RoadmapPhaseDto> Phases);

/// <summary>
/// Detailed phase within a roadmap, including resources, projects, and insights.
/// </summary>
public sealed record RoadmapPhaseDto(
    Guid Id,
    string Title,
    string Description,
    int Order,
    PhaseStatus Status,
    int DurationEstimateWeeks,
    List<PhaseResource> Resources,
    List<PhaseProject> Projects,
    List<string> Insights,
    DateTime CreatedAt,
    DateTime UpdatedAt);
