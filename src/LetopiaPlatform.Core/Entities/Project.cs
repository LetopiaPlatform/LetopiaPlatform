using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Entities;
public class Project : AuditableEntity
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public DifficultyLevel? DifficultyLevel { get; set; }
    public ProjectStatus Status { get; set; }

    public bool IsPublic { get; set; } = true;

    public DateTime StartDate { get; set; }
    public DateTime Deadline { get; set; }


    public List<string> RequiredSkills { get; set; } = [];
    public List<string> Goals { get; set; } = [];

    public List<string> TimelineEvents { get; set; } = [];


    //------------------------------------------------
    public Guid CategoryId { get; set; }

    public virtual ProjectCategory Category { get; set; } = null!;

    public Guid OwnerId { get; set; }
    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<ProjectMember> Members { get; set; } = new HashSet<ProjectMember>();

    public virtual ICollection<ProjectMilestoneDetails> Milestones { get; set; } = new HashSet<ProjectMilestoneDetails>();
}
