using LetopiaPlatform.Core.Common;

namespace LetopiaPlatform.Core.Entities;
public class CommunityTaskCategory : AuditableEntity
{
    public required string Name { get; set; }

    public string ColorHex { get; set; } = "#6366f1";

    public string? IconKey { get; set; }

    public Guid CommunityId { get; set; }
    public Community Community { get; set; } = null!;
    // navigation property
    public ICollection<CommunityTask> Tasks { get; set; } = [];
}
