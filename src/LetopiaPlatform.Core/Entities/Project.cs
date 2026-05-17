using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Entities;
public class Project : AuditableEntity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DifficultyLevel? DifficultyLevel { get; set; }
    public ProjectStatus Status { get; set; }

    public bool IsPublic { get; set; } = true;



    public List<string> RequiredSkills { get; set; } = [];



    //------------------NavigationProperty------------------------------
    public Guid CategoryId { get; set; }

    public virtual ProjectCategory Category { get; set; } = null!;

    public Guid OwnerId { get; set; }
    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<ProjectMember> Members { get; set; } = new HashSet<ProjectMember>();

    public virtual ICollection<ProjectMilestoneDetails> Milestones { get; set; } = new HashSet<ProjectMilestoneDetails>();


    public virtual ICollection<ProjectResource> Resources { get; set; } = new HashSet<ProjectResource>();


    public int CalculatedProgress => Milestones.Count > 0
    ? (int)((double)Milestones.Count(m => m.Status == MilestoneStatus.Completed) / Milestones.Count * 100)
    : 0;

}
public class ProjectResource : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsFile { get; set; }

    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; } = null!;
}

