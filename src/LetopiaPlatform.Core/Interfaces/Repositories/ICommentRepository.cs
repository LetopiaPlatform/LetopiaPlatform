
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities;

namespace LetopiaPlatform.Core.Interfaces.Repositories;
/// <summary>
/// Repository interface for managing <see cref="Comment"/> entities.
/// Includes comment-specific queries like counting reactions and paginated retrieval by post.
/// </summary>
public interface ICommentRepository : IGenericRepository<Comment>
{
    /// <summary>
    /// Gets a paginated list of comments for a specific post.
    /// </summary>
    /// <param name="postId">The post identifier.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A paginated result of <see cref="Comment"/> entities.</returns>
    Task<PaginatedResult<Comment>> GetCommentsByPostIdAsync(Guid postId, int page, int pageSize);

    /// <summary>
    /// Gets the total number of reactions for a specific comment.
    /// </summary>
    /// <param name="commentId">The comment identifier.</param>
    /// <returns>The number of reactions associated with the comment.</returns>
    Task<int> GetReactionCountAsync(Guid commentId);
}
