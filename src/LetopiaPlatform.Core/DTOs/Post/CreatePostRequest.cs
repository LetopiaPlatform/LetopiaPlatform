using LetopiaPlatform.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.DTOs.Post;


/// <summary>
/// Request DTO for creating a new post.
/// </summary>
public sealed record CreatePostRequest(
    string Title,
    string Content,
    PostType PostType = PostType.Discussion)
{
    /// <summary>
    /// Optional images attached to this post (max 10, each max 5 MB).
    /// Validated before any upload is attempted.
    /// </summary>
    public List<IFormFile> Images { get; init; } = [];

    /// <summary>
    /// Optional tags e.g. ["dotnet", "announcement"].
    /// Normalized to lowercase on save.
    /// </summary>
    public List<string> Tags { get; init; } = [];
}
