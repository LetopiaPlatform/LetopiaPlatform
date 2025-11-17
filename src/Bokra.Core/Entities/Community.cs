using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bokra.Core.Entities
{
    [Table("communities")]
    public class Community
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("slug")]
        public string Slug { get; set; }

        [Required]
        [Column("description")]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("topic_category")]
        public string TopicCategory { get; set; }

        [MaxLength(500)]
        [Column("icon_url")]
        public string? IconUrl { get; set; }

        // FK to users table
        [Required]
        [Column("created_by")]
        public Guid CreatedBy { get; set; }

        //// Navigation property
        
        //[ForeignKey("CreatedBy")]
        //public User CreatedByUser { get; set; }

        [Column("member_count")]
        public int MemberCount { get; set; } = 0;

        [Column("post_count")]
        public int PostCount { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //add some exter columns

        [MaxLength(500)]
        [Column("cover_image_url")]
        public string? CoverImageUrl { get; set; }

        [Column("is_private")]
        public bool IsPrivate { get; set; } = false;

        [Column("last_post_at")]
        public DateTime? LastPostAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
