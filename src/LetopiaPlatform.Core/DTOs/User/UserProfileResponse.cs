namespace LetopiaPlatform.Core.DTOs.User;

public sealed record UserProfileResponse(
    Guid Id,
    string FullName,
    string Email,
    string? Bio,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    string? AvatarUrl);
