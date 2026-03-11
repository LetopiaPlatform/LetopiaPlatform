using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.CommunityResourse;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing <see cref="CommunityResource"/> entities.
/// Extends <see cref="IGenericRepository{CommunityResource}"/> with resource-specific queries and actions.
/// </summary>
public interface IResourceRepository : IGenericRepository<CommunityResource>
{
    /// <summary>
    /// Retrieves a paginated list of resources belonging to a specific community.
    /// </summary>
    /// <param name="communityId">The identifier of the community.</param>
    /// <param name="query">Filtering and pagination parameters.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A paginated result of <see cref="CommunityResource"/> entities.</returns>
    Task<PaginatedResult<CommunityResource>> GetResourcesByCommunityAsync(
        Guid communityId,
        ResourceQueryParams query,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a resource by its ID, including related details
    /// such as tags, uploader information, and likes.
    /// </summary>
    /// <param name="resourceId">The identifier of the resource.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>The <see cref="CommunityResource"/> if found; otherwise, null.</returns>
    Task<CommunityResource?> GetByIdWithDetailsAsync(
        Guid resourceId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a paginated list of recommended resources within a community,
    /// filtered by resource type and ranked by weighted engagement score.
    /// </summary>
    /// <param name="communityId">The identifier of the community.</param>
    /// <param name="type">The type of resource to filter by.</param>
    /// <param name="page">The page number to retrieve (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A paginated result of recommended <see cref="CommunityResource"/> entities.</returns>
    Task<PaginatedResult<CommunityResource>> GetRecommendedAsync(
        Guid communityId,
        ResourceType type,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a resource by its URL. Used to prevent duplicate entries
    /// within the same community.
    /// </summary>
    /// <param name="url">The unique URL of the resource.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>The <see cref="CommunityResource"/> if found; otherwise, null.</returns>
    Task<CommunityResource?> GetByUrlAsync(
        string url,
        CancellationToken ct = default);

    /// <summary>
    /// Checks whether a resource has been liked by a specific user.
    /// </summary>
    /// <param name="resourceId">The identifier of the resource.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>True if the user has liked the resource; otherwise, false.</returns>
    Task<bool> IsLikedByUserAsync(
        Guid resourceId,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Atomically increments the view count for a resource.
    /// Uses <c>ExecuteUpdateAsync</c> — no SaveChanges call required after this.
    /// </summary>
    /// <param name="resourceId">The identifier of the resource.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    Task IncrementViewsAsync(
        Guid resourceId,
        CancellationToken ct = default);

    /// <summary>
    /// Adds a like from a user to a resource by inserting a <c>ResourceLike</c> row
    /// and atomically incrementing the denormalized <c>LikesCount</c> counter.
    /// Caller must check <see cref="IsLikedByUserAsync"/> first to avoid a duplicate key error.
    /// SaveChanges is handled by the caller via <c>IUnitOfWork</c>.
    /// </summary>
    /// <param name="resourceId">The identifier of the resource.</param>
    /// <param name="userId">The identifier of the user who is liking the resource.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    Task AddLikeAsync(
        Guid resourceId,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Removes a like from a user for a resource by deleting the <c>ResourceLike</c> row
    /// and atomically decrementing the denormalized <c>LikesCount</c> counter.
    /// SaveChanges is handled by the caller via <c>IUnitOfWork</c>.
    /// </summary>
    /// <param name="resourceId">The identifier of the resource.</param>
    /// <param name="userId">The identifier of the user who is removing the like.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    Task RemoveLikeAsync(
        Guid resourceId,
        Guid userId,
        CancellationToken ct = default);
}
