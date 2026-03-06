
namespace LetopiaPlatform.Core.DTOs.Agent;

/// <summary>
/// Represents a hands-on project within a roadmap phase.
/// Stored as a jsonb column on RoadmapPhase.
/// </summary>
public class PhaseProject
{
    /// <summary>
    /// Project title (e.g., "Build a REST API with authentication").
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Brief description of what the project covers and its learning objectives.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Difficulty level (e.g., "Beginner", "Intermediate", "Advanced").
    /// </summary>
    public required string Difficulty { get; set; }

    /// <summary>
    /// Ordered list of milestones, each containing granular tasks.
    /// </summary>
    public List<ProjectMilestone> Milestones { get; set; } = [];
}
