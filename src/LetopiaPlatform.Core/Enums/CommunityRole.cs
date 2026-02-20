namespace LetopiaPlatform.Core.Enums;

/// <summary>
/// Roles a user can hold within a community.
/// Stored as a string in the database.
/// </summary>
public enum CommunityRole
{
    Member,
    Moderator,
    Owner
}