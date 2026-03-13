using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.DTOs.Post;

/// <summary>
/// Request DTO for updating an existing post.
/// Null scalar fields are left unchanged (partial update pattern).
/// </summary>
public sealed record UpdatePostRequest
{
    public string? Title { get; init; }
    public string? Content { get; init; }

    /// <summary>
    /// New images to append. Existing images are kept unless their URLs
    /// appear in <see cref="RemoveImageUrls"/>.
    /// </summary>
    public List<IFormFile> AddImages { get; init; } = [];

    /// <summary>
    /// Exact URLs of existing images to remove from the post.
    /// Pass an empty list to keep all current images.
    /// </summary>
    public List<string> RemoveImageUrls { get; init; } = [];

    /// <summary>
    /// Full replacement tag list. Null = keep existing. Empty = remove all.
    /// </summary>
    public List<string>? Tags { get; init; }
}
