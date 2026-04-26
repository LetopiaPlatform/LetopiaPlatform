using LetopiaPlatform.Core.Common;

namespace LetopiaPlatform.Core.Entities;
public class ProjectMilestoneDetails : AuditableEntity
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }

    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; } = null!;
}
