using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Entities;
public class ProjectMember
{
    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; } = null!;

    public Guid MemberId { get; set; }
    public virtual User Member { get; set; } = null!;

    public ProjectMemberRole Role { get; set; } = ProjectMemberRole.Contributor;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

