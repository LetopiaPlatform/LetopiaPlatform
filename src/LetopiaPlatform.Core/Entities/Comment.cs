using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LetopiaPlatform.Core.Entities
{
    [Table("comments")]
    public class Comment
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        // FK to Post
        [Required]
        [Column("post_id")]
        public Guid PostId { get; set; }

        //[ForeignKey("PostId")]
        //public Post Post { get; set; }

        // FK to User (Author)
        [Required]
        [Column("author_id")]
        public Guid AuthorId { get; set; }

        //[ForeignKey("AuthorId")]
        //public User Author { get; set; }

        [Required]
        [Column("content")]
        public string Content { get; set; }

        [Column("upvotes")]
        public int Upvotes { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }



}
