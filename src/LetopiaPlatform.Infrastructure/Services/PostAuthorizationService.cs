
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;

namespace LetopiaPlatform.Infrastructure.Services;

/// <summary>
/// Default implementation of <see cref="IPostAuthorizationService"/>.
/// Handles post authorization rules based on post type
/// and the user's role inside the community.
/// </summary>
public sealed class PostAuthorizationService : IPostAuthorizationService
{
    /// <inheritdoc />
    public bool CanCreate(PostType postType, UserCommunity user)
    {
        if (user is null)
            return false;

        return postType switch
        {
            PostType.Discussion =>
                CanCreateDiscussion(user),

            PostType.Announcement =>
                CanCreateAnnouncement(user),

            _ => false
        };
    }

    /// <inheritdoc />
    public bool CanUpdate(PostType postType, UserCommunity user)
    {
        if (user is null)
            return false;

        return postType switch
        {
            PostType.Discussion =>
                user.Role is CommunityRole.Owner
                             or CommunityRole.Moderator,

            PostType.Announcement =>
                user.Role is CommunityRole.Owner
                             or CommunityRole.Moderator,

            _ => false
        };
    }

    /// <inheritdoc />
    public bool CanDelete(PostType postType, UserCommunity user)
    {
        if (user is null)
            return false;

        return user.Role is CommunityRole.Owner;
    }

    // ---------- Private Rules ----------

    private static bool CanCreateDiscussion(UserCommunity user)
        => user.Role is CommunityRole.Owner
                       or CommunityRole.Moderator
                       or CommunityRole.Member;

    private static bool CanCreateAnnouncement(UserCommunity user)
        => user.Role is CommunityRole.Owner
                       or CommunityRole.Moderator;
}
