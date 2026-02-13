using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities.Identity;

namespace LetopiaPlatform.Core.Entities;

public class UserCommunity : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid CommunityId { get; set; }
    public Community Community { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}