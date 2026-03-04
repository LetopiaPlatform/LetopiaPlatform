using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.DTOs.Post;

/// <summary>
/// Request DTO for updating an existing post.
/// All fields are optional for partial update.
/// </summary>
public sealed record UpdatePostRequest(
    string? Title = null,
    string? Content = null,
    IFormFile? PostImage = null
   );
