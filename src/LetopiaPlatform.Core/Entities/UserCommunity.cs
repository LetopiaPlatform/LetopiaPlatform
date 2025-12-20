using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace LetopiaPlatform.Core.Entities
{
    [Table("user_communities")]
    public class UserCommunity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        // FK to Users
        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        //[ForeignKey("UserId")]
        //public User User { get; set; }

        // FK to Communities
        [Required]
        [Column("community_id")]
        public Guid CommunityId { get; set; }

        //[ForeignKey("CommunityId")]
        //public Community Community { get; set; }

        [Column("joined_at")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
