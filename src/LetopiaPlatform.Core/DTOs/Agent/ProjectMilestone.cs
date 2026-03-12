
namespace LetopiaPlatform.Core.DTOs.Agent;

/// <summary>
/// Represents a milestone within a phase project, containing a set of actionable tasks.
/// Stored as part of PhaseProject's JSON structure in the database.
/// </summary>
public class ProjectMilestone
{
    /// <summary>
    /// The milestone title (e.g., "Set up project structure").
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Ordered list of tasks to complete this milestone
    /// (e.g., ["Initialize repo", "Configure CI pipeline", "Add README"]).
    /// </summary>
    public List<string> Tasks { get; set; } = [];
}
