using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Enums;
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
    /// ====================================================
    public PrivacySettings PrivacySettings { get; set; } = new();
    public string? Location { get; set; }
    public NotificationPreferences NotificationPreferences { get; set; } = new();
    public List<SocialLink>? SocialLinks { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    public List<string> Interests { get; set; } = new();

    /// ====================================================

    public int TotalPoints { get; set; }
    public int CurrentStreak { get; set; }

    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    //Navigation property for projects owned by the user
    public virtual ICollection<Project> OwnedProjects { get; set; } = new HashSet<Project>();

    public virtual ICollection<ProjectMember> ProjectMemberships { get; set; } = new HashSet<ProjectMember>();
    public virtual ICollection<UserRefreshToken> RefreshTokens { get; set; } = new HashSet<UserRefreshToken>();
}
