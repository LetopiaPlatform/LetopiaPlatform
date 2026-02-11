using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LetopiaPlatform.Core.Entities.Identity;

namespace LetopiaPlatform.Core.Entities;

[Table("comments")]
public class Comment
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("post_id")]
    public Guid PostId { get; set; }

    [ForeignKey("PostId")]
    public Post Post { get; set; } = null!;

    [Required]
    [Column("author_id")]
    public Guid AuthorId { get; set; }

    [ForeignKey("AuthorId")]
    public User Author { get; set; } = null!;

    [Required]
    [Column("content")]
    public required string Content { get; set; }

    [Column("upvotes")]
    public int Upvotes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}