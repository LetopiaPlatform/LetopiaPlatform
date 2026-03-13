using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Interfaces.Repositories;

/// <summary>
/// Data access operations for posts.
/// Does NOT manage persistence — use IUnitOfWork for SaveChanges and transactions.
/// </summary>
public interface IPostRepository : IGenericRepository<Post>
{
    /// <summary>Returns the number of non-deleted comments on a post.</summary>
    Task<int> GetCommentCountAsync(Guid postId, CancellationToken ct = default);

    /// <summary>Returns the reaction count for a post, optionally filtered by type.</summary>
    Task<int> GetReactionCountAsync(Guid postId, ReactionType? type = null, CancellationToken ct = default);

    /// <summary>
    /// Returns a paginated, sorted, optionally-searched page of posts in a channel.
    /// </summary>
    Task<PaginatedResult<Post>> GetPagedAsync(
        PaginatedQuery query,
        Guid communityId,
        Guid channelId,
        string? search,
        string? sortBy,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all pinned posts in a channel, ordered newest first.
    /// Pinned posts are never paginated — there should be very few of them.
    /// </summary>
    Task<IReadOnlyList<Post>> GetPinnedAsync(
        Guid communityId,
        Guid channelId,
        CancellationToken ct = default);


}
