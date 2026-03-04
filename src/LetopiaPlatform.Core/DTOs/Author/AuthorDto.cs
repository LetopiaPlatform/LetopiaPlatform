namespace LetopiaPlatform.Core.DTOs.Author;

/// <summary>
/// Represents an author of a post or comment.
/// </summary>
public record AuthorDto(
    Guid Id,
    string FullName,
    string? AvatarUrl);
