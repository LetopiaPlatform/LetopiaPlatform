
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Entities;
public class Reaction : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string? TargetType { get; set; }    // "Post" or "Comment"
    public Guid TargetId { get; set; }
    public ReactionType ReactionType { get; set; }
    public DateTime CreatedAt { get; set; }

}
