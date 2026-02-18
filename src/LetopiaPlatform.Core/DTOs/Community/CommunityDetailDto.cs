namespace LetopiaPlatform.Core.DTOs.Community;

/// <summary>
/// Full community detail for single-resource endpoints.
/// Includes membership context and channel list.
/// </summary>
public sealed record CommunityDetailDto(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string TopicCategory,
    string? IconUrl,
    string? CoverImageUrl,
    int MemberCount,
    int PostCount,
    bool IsPrivate,
    DateTime CreatedAt,
    DateTime? LastPostAt,
    bool IsMember,
    string? UserRole,
    List<ChannelSummaryDto> Channels);