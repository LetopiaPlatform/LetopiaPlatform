using LetopiaPlatform.Core.Common;

namespace LetopiaPlatform.Core.Entities;

/// <summary>
/// Sub-section within a community, (e.g., "Beginner Questions", "Project Showcases").
/// Each group has a unique slug scoped to its parent community.
/// </summary>
public class Group : BaseEntity
{
    public Guid CommunityId { get; set; }
    public Community Community { get; set; } = null!;

    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public int PostCount { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation collections
    public ICollection<Post> Posts { get; set; } = [];
}