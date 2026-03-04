
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Interfaces.Repositories;


    /// <summary>
    /// Repository interface for managing <see cref="Post"/> entities.
    /// Includes post-specific queries like comment count and reaction count.
    /// </summary>
    public interface IPostRepository : IGenericRepository<Post>
    {
        /// <summary>
        /// Gets the total number of comments for a specific post.
        /// </summary>
        /// <param name="postId">The unique identifier of the post.</param>
        /// <returns>The number of comments associated with the post.</returns>
        Task<int> GetCommentCountAsync(Guid postId);

        /// <summary>
        /// Gets the total number of reactions for a specific post.
        /// </summary>
        /// <param name="postId">The unique identifier of the post.</param>
        /// <param name="type">
        /// Optional. If specified, filters the reactions by <see cref="ReactionType"/> (e.g., Upvote, Downvote).
        /// </param>
        /// <returns>The count of reactions matching the criteria.</returns>
        Task<int> GetReactionCountAsync(Guid postId, ReactionType? type = null);
    }

