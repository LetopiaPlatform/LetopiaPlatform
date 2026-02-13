namespace LetopiaPlatform.Core.Community;

/// <summary>
/// Request DTO for creating a new group within a community.
/// Slug is auto-generated from <see cref="Name"/> by the service layer.
/// </summary>
public sealed record CreateGroupRequest(
    string Name,
    string? Description = null);
