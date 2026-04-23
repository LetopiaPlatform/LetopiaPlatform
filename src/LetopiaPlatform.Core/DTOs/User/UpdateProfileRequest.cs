using LetopiaPlatform.Core.Common;

namespace LetopiaPlatform.Core.DTOs.User;

public sealed record UpdateProfileRequest(
    // Basic profile
    string? FullName,

    string? Bio,
    string? PhoneNumber,
    string? Location,
    List<SocialLinkDto>? SocialLinks,
    List<string>? Interests,
    List<string>? Skills
    );
public sealed class SocialLinkDto
{
    public string Provider { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
