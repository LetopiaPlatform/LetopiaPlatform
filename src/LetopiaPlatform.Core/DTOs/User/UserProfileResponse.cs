using LetopiaPlatform.Core.Common;

namespace LetopiaPlatform.Core.DTOs.User;

public sealed record UserProfileResponse(
    Guid Id,
    string FullName,
    string Email,
    string? Bio,
    string? PhoneNumber,
    string? AvatarUrl,
    string? Location,
    string Role,
    bool EmailVerified,
    int TotalPoints,
    int CurrentStreak,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    // Preferences
    NotificationPreferences NotificationPreferences,
    List<SocialLink> SocialLinks,
    List<string>Skills,
    List<string> Interests,
    // Privacy
    PrivacySettings PrivacySettings);
