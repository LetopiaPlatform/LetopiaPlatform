namespace LetopiaPlatform.Core.DTOs.Community;

/// <summary>
/// Channel representation used within <see cref="CommunityDetailDto"/>.
/// Includes nested sub-channels for self-referencing hierarchy.
/// </summary>
public sealed record ChannelSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string ChannelType,
    string? Description,
    int PostCount,
    bool IsDefault,
    bool IsArchived,
    bool AllowMemberPosts,
    bool AllowComments,
    int DisplayOrder,
    List<ChannelSummaryDto> SubChannels);
