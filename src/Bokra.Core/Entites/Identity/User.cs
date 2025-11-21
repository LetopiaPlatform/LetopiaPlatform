

using Microsoft.AspNetCore.Identity;

namespace Bokra.Core.Entites.Identity
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // PasswordHash موجود بالفعل في IdentityUser
        // نفس الكلام ل Email, UserName, PhoneNumber...

        public string AvatarUrl { get; set; }
        public string Bio { get; set; }

        public string Role { get; set; } = "Learner"; // Learner / Guide / Architect

        public bool EmailVerified { get; set; } = false;

        public int TotalPoints { get; set; } = 0;
        public int CurrentStreak { get; set; } = 0;

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
