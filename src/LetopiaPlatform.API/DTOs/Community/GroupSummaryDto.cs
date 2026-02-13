namespace LetopiaPlatform.Core.Community;

/// <summary>
/// Lightweight group representation used within <see cref="CommunityDetailDto"/>.
/// </summary>
public sealed record GroupSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    int PostCount);
