using System;

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LetopiaPlatform.Core.Enums;


namespace LetopiaPlatform.Core.Entities
{
    [Table("posts")]
    public class Post
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        // FK to Community
        [Required]
        [Column("community_id")]
        public Guid CommunityId { get; set; }

        //[ForeignKey("CommunityId")]
        //public Community Community { get; set; }

        // FK to User (Author)
        [Required]
        [Column("author_id")]
        public Guid AuthorId { get; set; }

        //[ForeignKey("AuthorId")]
        //public User Author { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("title")]
        public string Title { get; set; }

        [Required]
        [Column("content")]
        public string Content { get; set; }

        [MaxLength(20)]
        [Column("post_type")]
        public PostType PostType { get; set; } = PostType.Discussion; // Discussion, Question, Resource

        [Column("upvotes")]
        public int Upvotes { get; set; } = 0;

        [Column("comment_count")]
        public int CommentCount { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //exter columns
        [Column("is_pinned")]
        public bool IsPinned { get; set; } = false;

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("views_count")]
        public int ViewsCount { get; set; } = 0;

    }
}
