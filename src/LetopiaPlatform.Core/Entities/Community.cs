using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities.Identity;

namespace LetopiaPlatform.Core.Entities;

public class Community : AuditableEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string Description { get; set; }
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string? IconUrl { get; set; }

    public Guid CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public int MemberCount { get; set; }
    public int PostCount { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime? LastPostAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation collections
    public ICollection<Post> Posts { get; set; } = [];
    public ICollection<UserCommunity> Members { get; set; } = [];
    public ICollection<Channel> Channels { get; set; } = [];
}