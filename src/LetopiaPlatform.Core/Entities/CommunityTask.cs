using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Entities;
public class CommunityTask : AuditableEntity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public CommunityTaskStatus Status { get; set; } = CommunityTaskStatus.Active;
    public DateTime Deadline { get; set; }

    public Guid CommunityId { get; set; }
    public Community Community { get; set; } = null!;

    public Guid? CategoryId { get; set; }
    public CommunityTaskCategory? Category { get; set; }

    // navigation property
    public ICollection<UserTaskProgress> UserProgresses { get; set; } = [];
}
