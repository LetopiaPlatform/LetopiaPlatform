using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;

namespace LetopiaPlatform.Infrastructure.Services;

/// <summary>
/// Enforces post creation, update, and deletion rules based on
/// the combination of post type, channel type, and the actor's community role.
/// </summary>
public sealed class PostAuthorizationService : IPostAuthorizationService
{
    // ── Create ────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public bool CanCreate(PostType postType, ChannelType channelType, UserCommunity? user)
    {
        if (user is null) return false;

        return (postType, channelType) switch
        {
            // Discussions are allowed in General and Discussion channels, by any member+
            (PostType.Discussion, ChannelType.Discussion)
                => user.Role is CommunityRole.Owner
                             or CommunityRole.Moderator
                             or CommunityRole.Member,

            // Announcements are only allowed in Announcement channels, by Owner/Moderator
            (PostType.Announcement, ChannelType.Announcement)
                => user.Role is CommunityRole.Owner
                             or CommunityRole.Moderator,

            // Any other postType/channelType combination is rejected
            _ => false,
        };
    }

    // ── Update ────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    /// <remarks>
    /// The author can always edit their own post (checked in the service before calling this).
    /// This method answers: can a non-author privileged user edit this post?
    /// </remarks>
    public bool CanUpdate(PostType postType, ChannelType channelType, UserCommunity? user)
    {
        if (user is null) return false;

        return (postType, channelType) switch
        {
            // Owners and Moderators can edit any Discussion in any allowed channel
            (PostType.Discussion, ChannelType.Discussion)
                => user.Role is CommunityRole.Owner
                             or CommunityRole.Moderator,

            // Only Owners and Moderators can edit Announcements
            (PostType.Announcement, ChannelType.Announcement)
                => user.Role is CommunityRole.Owner
                             or CommunityRole.Moderator,

            _ => false,
        };
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    /// <remarks>
    /// The author can always delete their own post (checked in the service before calling this).
    /// This method answers: can a non-author privileged user delete this post?
    /// </remarks>
    public bool CanDelete(PostType postType, ChannelType channelType, UserCommunity? user)
    {
        if (user is null) return false;

        return (postType, channelType) switch
        {
            // Owners can delete any Discussion
            (PostType.Discussion, ChannelType.Discussion)
                => user.Role is CommunityRole.Owner
                             or CommunityRole.Moderator,

            // Only Owners can delete Announcements
            (PostType.Announcement, ChannelType.Announcement)
                => user.Role is CommunityRole.Owner,

            _ => false,
        };
    }
    public bool CanPin(UserCommunity membership) =>
    membership.Role is CommunityRole.Owner  or CommunityRole.Moderator
                             or CommunityRole.Member;
}
