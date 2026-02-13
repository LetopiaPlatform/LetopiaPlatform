using LetopiaPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace LetopiaPlatform.Core.Entities.Identity;

public class User : IdentityUser<Guid>, IAuditable
{
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }

    public string Role { get; set; } = "Learner"; // Learner / Guide / Architect

    public bool EmailVerified { get; set; }

    public int TotalPoints { get; set; }
    public int CurrentStreak { get; set; }

    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
