namespace LetopiaPlatform.Core.DTOs.User;

public sealed record UpdateProfileRequest(
    string? FullName,
    string? Email,
    string? Bio,
    string? PhoneNumber);
