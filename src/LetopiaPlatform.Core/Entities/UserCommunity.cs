using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LetopiaPlatform.Core.Entities.Identity;

namespace LetopiaPlatform.Core.Entities
{
    [Table("user_communities")]
    public class UserCommunity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        [Column("community_id")]
        public Guid CommunityId { get; set; }

        [ForeignKey("CommunityId")]
        public Community Community { get; set; } = null!;

        [Column("joined_at")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}