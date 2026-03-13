using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Interfaces;
/// <summary>
/// Provides authorization rules related to post creation and actions
/// based on the <see cref="PostType"/> and the current user context.
/// </summary>
public interface IPostAuthorizationService
{
    /// <summary>
    /// Determines whether the current user is allowed to create
    /// a post of the specified <paramref name="postType"/>.
    /// </summary>
    /// <param name="postType">
    /// The type of the post to be created
    /// (e.g., Discussion, Announcement).
    /// </param>
    ///   /// <param name="channelType">
    /// The type of the channel 
    /// (e.g., Discussion, Announcement).
    /// </param>
    /// <param name="user">
    /// The current user context containing identity and role information.
    /// </param>
    /// <returns>
    /// <c>true</c> if the user is authorized to create the post;
    /// otherwise, <c>false</c>.
    /// </returns>
    bool CanCreate(PostType postType,ChannelType channelType, UserCommunity user);


    /// <summary>
    /// Determines whether the current user is allowed to update a post.
    /// </summary>
    bool CanUpdate(PostType postType, ChannelType channelType, UserCommunity user);

    /// <summary>
    /// Determines whether the current user is allowed to delete a post.
    /// </summary>
    bool CanDelete(PostType postType, ChannelType channelType, UserCommunity user);
}
