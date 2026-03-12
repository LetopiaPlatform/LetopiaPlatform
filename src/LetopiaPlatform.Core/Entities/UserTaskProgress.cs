using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities.Identity;

namespace LetopiaPlatform.Core.Entities;
public class UserTaskProgress : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid TaskId { get; set; }
    public CommunityTask Task { get; set; } = null!;

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
