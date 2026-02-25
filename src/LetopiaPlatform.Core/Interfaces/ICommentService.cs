using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Comment;

namespace LetopiaPlatform.Core.Interfaces;
/// <summary>
/// Provides operations for managing comments on posts, including creation, retrieval, update, and soft-deletion.
/// Supports polymorphic reactions via TargetType mapping.
/// </summary>
public interface ICommentService
{
    #region Create

    /// <summary>
    /// Creates a new comment on a discussion post.
    /// </summary>
    /// <param name="postId">The ID of the post to comment on.</param>
    /// <param name="request">The comment creation request.</param>
    /// <param name="userId">The ID of the user creating the comment.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>The created comment data.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the post does not exist or is deleted.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the post type does not allow comments.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the user is not allowed to comment.</exception>
    Task<CommentDto> CreateAsync(Guid postId, CreateCommentRequest request, Guid userId, CancellationToken ct = default);

    #endregion

    #region Read

    /// <summary>
    /// Returns a paginated list of comments for a given post.
    /// </summary>
    /// <param name="postId">The ID of the post.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of comments per page.</param>
    /// <param name="search">Optional search string to filter comments.</param>
    /// <param name="currentUserId">Optional current user ID to include their reactions.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A paginated result of comment data transfer objects.</returns>
    Task<PaginatedResult<CommentDto>> ListAsync(
        Guid postId,
        int page = 1,
        int pageSize = 50,
        string? search = null,
        Guid? currentUserId = null,
        CancellationToken ct = default);

    #endregion

    #region Update

    /// <summary>
    /// Updates the content of a comment. Only the author or a community moderator can update a comment.
    /// </summary>
    /// <param name="commentId">The ID of the comment to update.</param>
    /// <param name="request">The comment update request.</param>
    /// <param name="userId">The ID of the user performing the update.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>The updated comment data.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the comment does not exist or is deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the user is not authorized to update the comment.</exception>
    Task<CommentDto> UpdateAsync(Guid commentId, UpdateCommentRequest request, Guid userId, CancellationToken ct = default);

    #endregion

    #region Delete

    /// <summary>
    /// Soft-deletes a comment. Only the author or a community moderator can delete a comment.
    /// </summary>
    /// <param name="commentId">The ID of the comment to delete.</param>
    /// <param name="userId">The ID of the user performing the deletion.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the comment does not exist or is already deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the user is not authorized to delete the comment.</exception>
    Task DeleteAsync(Guid commentId, Guid userId, CancellationToken ct = default);

    #endregion
}
