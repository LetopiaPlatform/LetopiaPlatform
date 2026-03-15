using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;

namespace LetopiaPlatform.Core.Entities;
public class CommunityResource : ISoftDeletable
{
    public Guid Id { get; set; }

    public string Title { get; set; } = default!;

    public string Url { get; set; } = default!;

    public ResourceType Type { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? Description { get; set; }

    // ── Ownership & community ─────────────────────────────────────────────────

    public Guid CommunityId { get; set; }

    /// <summary>Id of the member who uploaded this resource.</summary>
    public Guid CreatedBy { get; set; }

    // ── Engagement counters (denormalized for fast reads) ─────────────────────

    public int ViewsCount { get; set; }

    public int LikesCount { get; set; }

    // ── Soft-delete & auditing ────────────────────────────────────────────────

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // ── Navigation Properties ─────────────────────────────────────────────────

    public Community Community { get; set; } = default!;

    /// <summary>The member who uploaded this resource.</summary>
    public User UploadedBy { get; set; } = default!;

    public ICollection<ResourceLike> Likes { get; set; } = new List<ResourceLike>();


}
