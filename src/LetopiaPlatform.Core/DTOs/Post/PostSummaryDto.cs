using LetopiaPlatform.Core.DTOs.Author;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Post;

/// <summary>
/// Summary information of a post.
/// </summary>
public record PostSummaryDto(
    Guid Id,
    string Title,
    string Content, // truncated content
    string? PostImageUrl,
    AuthorDto AuthorInfo,
    PostType PostType,
    int Upvotes,
    int CommentCount,
    int ViewsCount,
    bool IsPinned,
    DateTime CreatedAt,
    string? CurrentUserReaction);
