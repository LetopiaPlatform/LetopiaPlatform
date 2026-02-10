using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Entities
{
    [Table("posts")]
    public class Post
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("community_id")]
        public Guid CommunityId { get; set; }

        [ForeignKey("CommunityId")]
        public Community Community { get; set; } = null!;

        [Required]
        [Column("author_id")]
        public Guid AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public User Author { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        [Column("title")]
        public required string Title { get; set; }

        [Required]
        [Column("content")]
        public required string Content { get; set; }

        [MaxLength(20)]
        [Column("post_type")]
        public PostType PostType { get; set; } = PostType.Discussion;

        [Column("upvotes")]
        public int Upvotes { get; set; }

        [Column("comment_count")]
        public int CommentCount { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_pinned")]
        public bool IsPinned { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        [Column("views_count")]
        public int ViewsCount { get; set; }
    }
}