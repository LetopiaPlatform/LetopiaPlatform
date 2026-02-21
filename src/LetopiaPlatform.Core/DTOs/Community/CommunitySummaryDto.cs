namespace LetopiaPlatform.Core.DTOs.Community;

/// <summary>
/// Lightweight community representation for list endpoints.
/// </summary>
public sealed record CommunitySummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    Guid CategoryId,
    string CategoryName,
    string? IconUrl,
    string? CoverImageUrl,
    int MemberCount,
    int PostCount,
    bool IsPrivate,
    DateTime CreatedAt);