using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.DTOs.Community;

/// <summary>
/// Request DTO for updating an existing community.
/// All fields are optional — null means "don't change".
/// </summary>
public sealed record UpdateCommunityRequest(
    string? Name,
    string? Description,
    IFormFile? CoverImage,
    bool? IsPrivate,
    List<string>? Rules = null);
    bool? IsPrivate);
