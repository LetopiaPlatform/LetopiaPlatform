namespace LetopiaPlatform.Core.DTOs.Community;

/// <summary>
/// Represents a community member with their role and join date.
/// Used in the members list endpoint.
/// </summary>
public sealed record MemberDto(
    Guid UserId,
    string FullName,
    string? AvatarUrl,
    string Role,
    DateTime JoinedAt);
