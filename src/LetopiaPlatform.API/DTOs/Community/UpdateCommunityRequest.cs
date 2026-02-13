namespace LetopiaPlatform.Core.Community;

/// <summary>
/// Request DTO for updating an existing community.
/// All fields are optional — null means "don't change".
/// </summary>
public sealed record UpdateCommunityRequest(
    string? Name,
    string? Description,
    string? IconUrl,
    string? CoverImageUrl,
    bool? IsPrivate);
