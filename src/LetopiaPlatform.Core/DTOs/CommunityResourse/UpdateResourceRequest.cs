using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.CommunityResourse;

/// <param name="Title">New title. Leave null to keep the existing value.</param>
/// <param name="Description">New description. Leave null to keep the existing value.</param>
/// <param name="Url">
/// New URL. Leave null to keep the existing value.
/// When provided, the preview (thumbnail, title, description) is re-scraped
/// and the duplicate check is re-run within the same community.
/// </param>
/// <param name="Type">New resource type. Leave null to keep the existing value.</param>
/// <param name="Tags">
/// Full replacement tag list. Leave null to keep existing tags unchanged.
/// Pass an empty list to remove all tags.
/// </param>
public record UpdateResourceRequest(
    string? Title = null,
    string? Description = null,
    string? Url = null,
    ResourceType? Type = null,
    List<string>? Tags = null);
