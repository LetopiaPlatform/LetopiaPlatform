using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Entities;

/// <summary>
/// Represents a phase within a learning roadmap.
/// </summary>
public class RoadmapPhase : AuditableEntity
{
    /// <summary>
    /// Foreign key to the parent Roadmap.
    /// </summary>
    public Guid RoadmapId { get; set; }

    /// <summary>
    /// The title of this phase.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// A detailed description of the phase.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// The sequential order of this phase within the roadmap.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The current status of this phase.
    /// </summary>
    public PhaseStatus Status { get; set; } = PhaseStatus.NotStarted;

    /// <summary>
    /// Estimated duration in weeks to complete this phase.
    /// </summary>
    public int DurationEstimateWeeks { get; set; }

    /// <summary>
    /// Collection of learning resources for this phase (stored as JSONB).
    /// </summary>
    public List<PhaseResource> Resources { get; set; } = [];

    /// <summary>
    /// Collection of practical projects for this phase (stored as JSONB).
    /// </summary>
    public List<PhaseProject> Projects { get; set; } = [];

    /// <summary>
    /// Collection of key insights or takeaways for this phase (stored as JSONB).
    /// </summary>
    public List<string> Insights { get; set; } = [];

    /// <summary>
    /// Navigation property to the parent Roadmap.
    /// </summary>
    public Roadmap Roadmap { get; set; } = null!;
}
