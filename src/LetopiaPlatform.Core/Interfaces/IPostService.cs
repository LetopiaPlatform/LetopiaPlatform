using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// <returns>The created post details.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the community does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the user is not allowed to create posts.</exception>
    Task<PostDetailDto> CreateAsync(Guid communityId, Guid channelId, CreatePostRequest request, Guid userId, CancellationToken ct = default);

    #endregion

    #region Read

    /// <summary>
    /// Retrieves a paginated list of posts for a community.
    /// </summary>
    /// <param name="communityId">The ID of the community.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="search">Optional search keyword.</param>
    /// <param name="sortBy">Optional sort field (createdAt, upvotes, comments).</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A paginated list of post summaries.</returns>
    Task<PaginatedResult<PostSummaryDto>> ListAsync(Guid communityId, int page, int pageSize, string? search, string? sortBy, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the details of a post, including the current user's reaction if provided.
    /// </summary>
    /// <param name="postId">The ID of the post to retrieve.</param>
    /// <param name="currentUserId">Optional user ID to fetch user's reaction.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>The detailed post data.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the post does not exist or is deleted.</exception>
    Task<PostDetailDto> GetByIdAsync(Guid postId, Guid? currentUserId = null, CancellationToken ct = default);

    #endregion

    #region Update

    /// <summary>
    /// Updates a post. Only the author or community moderator can update the post.
    /// </summary>
    /// <param name="postId">The ID of the post to update.</param>
    /// <param name="request">The update request containing new title/content.</param>
    /// <param name="userId">The ID of the user performing the update.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>The updated post details.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the post does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the user is not authorized to update the post.</exception>
    Task<PostDetailDto> UpdateAsync(Guid postId, UpdatePostRequest request, Guid userId, CancellationToken ct = default);

    #endregion

    #region Delete

    /// <summary>
    /// Soft-deletes a post. Only the author or community moderator can delete the post.
    /// </summary>
    /// <param name="postId">The ID of the post to delete.</param>
    /// <param name="userId">The ID of the user performing the deletion.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the post does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the user is not authorized to delete the post.</exception>
    Task DeleteAsync(Guid postId, Guid userId, CancellationToken ct = default);

    #endregion
}
