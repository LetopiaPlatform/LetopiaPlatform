
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.CommunityResourse;
public record UpdateResourceRequest
{
    /// <summary>
    /// New title. Leave null to keep the existing value.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// New description. Leave null to keep the existing value.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// New URL. Leave null to keep the existing value.
    /// When provided, the preview (thumbnail, title, description) is re-scraped
    /// and the duplicate check is re-run within the same community.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// New resource type. Leave null to keep the existing value.
    /// </summary>
    public ResourceType? Type { get; init; }

    /// <summary>
    /// Full replacement tag list. Leave null to keep existing tags unchanged.
    /// Pass an empty list to remove all tags.
    /// </summary>
    public List<string>? Tags { get; init; }
}
