namespace LetopiaPlatform.Core.DTOs.User;

public sealed record UserSummaryDto(
    Guid Id,
    string FullName,
    string UserName,
    string? AvatarUrl);