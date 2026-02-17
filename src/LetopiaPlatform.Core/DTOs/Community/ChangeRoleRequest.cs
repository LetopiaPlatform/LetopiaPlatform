namespace LetopiaPlatform.Core.DTOs.Community;

/// <summary>
/// Request DTO for changing a member's role within a community.
/// Role must be one of: "Member", "Moderator", "Owner".
/// </summary>
public sealed record ChangeRoleRequest(string Role);
