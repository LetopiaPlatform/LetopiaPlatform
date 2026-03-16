using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.CommunityResourse;

/// <param name="Id">Unique identifier of the resource.</param>
/// <param name="Title">Display title of the resource.</param>
/// <param name="Url">The saved URL of the resource.</param>
/// <param name="Type">The resource type (e.g. Article, Video).</param>
/// <param name="UploadedBy">Brief info about who uploaded this resource.</param>
/// <param name="CreatedAt">When the resource was added.</param>
/// <param name="ThumbnailUrl">Optional og:image thumbnail.</param>
/// <param name="Description">Optional description.</param>
/// <param name="ViewsCount">Total view count.</param>
/// <param name="LikesCount">Total like count.</param>
/// <param name="IsLikedByCurrentUser">Whether the requesting user has liked this resource.</param>
/// <param name="Tags">Tags associated with this resource.</param>
public record ResourceDto(
    Guid Id,
    string Title,
    string Url,
    ResourceType Type,
    UploadedByDto UploadedBy,
    DateTime CreatedAt,
    string? ThumbnailUrl = null,
    string? Description = null,
    int ViewsCount = 0,
    int LikesCount = 0,
    bool IsLikedByCurrentUser = false,
    List<string>? Tags = null);
