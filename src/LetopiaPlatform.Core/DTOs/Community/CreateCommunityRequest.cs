namespace LetopiaPlatform.Core.DTOs.Community;

/// <summary>
/// Request DTO for creating a new community.
/// Slug is auto-generated from <see cref="Name"/> by the service layer.
/// </summary>
public sealed record CreateCommunityRequest(
    string Name,
    string Description,
    Guid CategoryId,
    bool IsPrivate = false,
    string? IconUrl = null);