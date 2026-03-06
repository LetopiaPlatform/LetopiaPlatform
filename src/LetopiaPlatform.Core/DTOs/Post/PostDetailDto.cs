using LetopiaPlatform.Core.DTOs.Author;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Post;

/// <summary>
/// Detailed information of a post.
/// </summary>
public record PostDetailDto(
    Guid Id,
    string Title,
    string Content,
    string? PostImageUrl,
    AuthorDto AuthorInfo,
    PostType PostType,
    int Upvotes,
    int CommentCount,
    int ViewsCount,
    bool IsPinned,
    DateTime CreatedAt,
    string? CurrentUserReaction,
    DateTime? UpdatedAt);
