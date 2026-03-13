using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Post;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Provides business logic operations for community posts, including creation, retrieval, updating, and deletion.
/// </summary>
public interface IPostService
{
    #region Create

    /// <summary>
    /// Creates a new post in a community channel.
    /// </summary>
    /// <param name="communityId">The ID of the target community.</param>
    /// <param name="channelId">The ID of the target channel.</param>
    /// <param name="request">The request containing post details.</param>
    /// <param name="userId">The ID of the user creating the post.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A result containing the created post details or error information.</returns>
    Task<Result<PostDetailDto>> CreateAsync(Guid communityId, Guid channelId, CreatePostRequest request, Guid userId, CancellationToken ct = default);

    #endregion

    #region Read

    /// <summary>
    /// Retrieves a paginated list of posts for a community.
    /// </summary>
    Task<Result<PaginatedResult<PostSummaryDto>>> ListAsync(Guid communityId, Guid channelId, int page, int pageSize, string? search, string? sortBy, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the details of a post, including the current user's reaction if provided.
    /// </summary>
    Task<Result<PostDetailDto>> GetByIdAsync(Guid postId, Guid? currentUserId = null, CancellationToken ct = default);

    /// <summary>
    /// Returns all pinned posts in a channel.
    /// </summary>
    Task<Result<IReadOnlyList<PostSummaryDto>>> GetPinnedAsync(Guid communityId, Guid channelId, CancellationToken ct = default);

    

    #endregion

    #region Update

    /// <summary>
    /// Updates a post. Only the author or community moderator can update the post.
    /// </summary>
    Task<Result<PostDetailDto>> UpdateAsync(Guid postId, UpdatePostRequest request, Guid userId, CancellationToken ct = default);

    #endregion

    #region Delete

    /// <summary>
    /// Soft-deletes a post. Only the author or community moderator can delete the post.
    /// </summary>
    Task<Result> DeleteAsync(Guid postId, Guid userId, CancellationToken ct = default);

    #endregion
}
