namespace LetopiaPlatform.Core.Community;

/// <summary>
/// Lightweight community representation for list endpoints.
/// </summary>
public sealed record CommunitySummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string TopicCategory,
    string? IconUrl,
    int MemberCount,
    int PostCount,
    bool IsPrivate,
    DateTime CreatedAt);
