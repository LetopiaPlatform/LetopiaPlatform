using LetopiaPlatform.Core.DTOs.Author;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Post;

/// <summary>
/// Summary information of a post shown in feed/list views.
/// Content is truncated to 200 characters.
/// </summary>
public record PostSummaryDto(
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
    List<string> Tags);
