using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Entities;

/// <summary>
/// An organizational unit within a community. ChannelType defines behavior (who can post, content style). Supports one level of nesting via ParentId
/// for sub-channels (e.g., Resources -> Books, Articles, Videos)
/// </summary>
public class Channel : AuditableEntity
{
    public Guid CommunityId { get; set; }
    public Community Community { get; set; } = null!;

    public Guid? ParentId {get; set; }
    public Channel? Parent { get; set; }

    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public ChannelType ChannelType { get; set; } = ChannelType.Discussion;

    public int DisplayOrder {get; set;}
    public int PostCount {get; set;}
    public bool IsDefault {get; set;}
    public bool IsArchived {get; set;}

    public bool AllowMemberPosts { get; set; } = true;
    public bool AllowComments { get; set; } = true;

    // Navigation collections
    public ICollection<Channel> SubChannels {get; set;} = [];
    public ICollection<Post> Posts {get; set;} = [];
}