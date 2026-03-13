using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Author;

/// <summary>
/// Represents the author of a post or comment,
/// including their role within the community context.
/// Null role means the author's membership could not be resolved
/// (e.g. they left the community after posting).
/// </summary>
public record AuthorDto(
    Guid Id,
    string FullName,
    string? AvatarUrl,
    CommunityRole? CommunityRole = null);
