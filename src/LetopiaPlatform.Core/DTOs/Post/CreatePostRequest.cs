using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Post;


/// <summary>
/// Request DTO for creating a new post.
/// </summary>
public sealed record CreatePostRequest(
    string Title,
    string Content,
    PostType PostType = PostType.Discussion);
