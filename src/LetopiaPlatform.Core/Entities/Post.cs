using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;

namespace LetopiaPlatform.Core.Entities;

public class Post : AuditableEntity, ISoftDeletable
{
    public Guid CommunityId { get; set; }
    public Community Community { get; set; } = null!;

    public Guid ChannelId { get; set; }
    public Channel Channel { get; set; } = null!;

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public required string Title { get; set; }
    public required string Content { get; set; }
    public PostType PostType { get; set; } = PostType.Discussion;

    public int Upvotes { get; set; }
    public int CommentCount { get; set; }
    public bool IsPinned { get; set; }
    public bool IsDeleted { get; set; }
    public int ViewsCount { get; set; }

    // Navigation collections
    public ICollection<Comment> Comments { get; set; } = [];
}