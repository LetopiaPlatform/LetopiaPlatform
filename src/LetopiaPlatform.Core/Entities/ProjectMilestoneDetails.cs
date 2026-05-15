using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Entities;
public class ProjectMilestoneDetails : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DurationText { get; set; } // مثال: "Week 1-2"
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;

    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; } = null!;
}
