using LetopiaPlatform.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.DTOs.Post;


/// <summary>
/// Request DTO for creating a new post.
/// </summary>
public sealed record CreatePostRequest(
    string Title,
    string Content,
    IFormFile? PostImage,
    PostType PostType = PostType.Discussion);
