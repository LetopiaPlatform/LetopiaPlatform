using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities.Identity;

namespace LetopiaPlatform.Core.Entities;

[Table("communities")]
public class Community : AuditableEntity
{
    [Required]
    [MaxLength(100)]
    [Column("name")]
    public required string Name { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("slug")]
    public required string Slug { get; set; }

    [Required]
    [Column("description")]
    public required string Description { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("topic_category")]
    public required string TopicCategory { get; set; }

    [MaxLength(500)]
    [Column("icon_url")]
    public string? IconUrl { get; set; }

    [Required]
    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    public User CreatedByUser { get; set; } = null!;

    [Column("member_count")]
    public int MemberCount { get; set; }

    [Column("post_count")]
    public int PostCount { get; set; }

    [MaxLength(500)]
    [Column("cover_image_url")]
    public string? CoverImageUrl { get; set; }

    [Column("is_private")]
    public bool IsPrivate { get; set; }

    [Column("last_post_at")]
    public DateTime? LastPostAt { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}