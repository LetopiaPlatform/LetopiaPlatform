using LetopiaPlatform.Core.DTOs.Author;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Post;

/// <summary>
/// Detailed information of a post including all images and tags.
/// </summary>
public record PostDetailDto(
    Guid Id,
    string Title,
    string Content,
    List<string> ImageUrls,
    AuthorDto AuthorInfo,
    PostType PostType,
    int Upvotes,
    int CommentCount,
    int ViewsCount,
    bool IsPinned,
    DateTime CreatedAt,
    string? CurrentUserReaction,
    DateTime? UpdatedAt,
    List<string> Tags);
