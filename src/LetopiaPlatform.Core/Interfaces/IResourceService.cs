using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.CommunityResourse;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Provides operations for managing community resources such as links, videos,
/// documents, and other shared materials.
/// </summary>
public interface IResourceService
{
    /// <summary>
    /// Creates a new resource inside a community.
    /// </summary>
    /// <param name="request">
    /// The resource creation request containing the URL, type, optional metadata,
    /// and tags associated with the resource.
    /// </param>
    /// <param name="communityId">The identifier of the community to add the resource to.</param>
    /// <param name="userId">The identifier of the user creating the resource.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the created <see cref="ResourceDto"/> if successful.
    /// </returns>
    Task<Result<ResourceDto>> CreateResourceAsync(
        CreateResourceRequest request,
        Guid communityId,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Updates an existing resource's metadata (title, description, url, type, tags).
    /// Only the original uploader, a community Owner, or a Moderator may update.
    /// When a new URL is provided, the duplicate check is re-run and the preview
    /// (thumbnail, title, description) is re-scraped from the new URL.
    /// </summary>
    /// <param name="resourceId">The identifier of the resource to update.</param>
    /// <param name="request">Fields to update. Null properties are left unchanged.</param>
    /// <param name="userId">The identifier of the user requesting the update.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the updated <see cref="ResourceDto"/> if successful.
    /// </returns>
    Task<Result<ResourceDto>> UpdateResourceAsync(
        Guid resourceId,
        UpdateResourceRequest request,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a paginated list of resources for a specific community.
    /// </summary>
    /// <param name="communityId">The identifier of the community whose resources should be returned.</param>
    /// <param name="query">Filtering and pagination parameters such as resource type, tag, page number, and page size.</param>
    /// <param name="currentUserId">
    /// The identifier of the currently authenticated user. Used to determine
    /// user-specific data such as whether the resource is liked.
    /// </param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a paginated list of <see cref="ResourceDto"/>.
    /// </returns>
    Task<Result<PaginatedResult<ResourceDto>>> GetResourcesAsync(
        Guid communityId,
        ResourceQueryParams query,
        Guid currentUserId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a single resource by its identifier.
    /// </summary>
    /// <param name="resourceId">The identifier of the resource.</param>
    /// <param name="currentUserId">
    /// The identifier of the current user used to resolve personalized data
    /// such as like status.
    /// </param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the requested <see cref="ResourceDto"/>.
    /// </returns>
    Task<Result<ResourceDto>> GetResourceAsync(
        Guid resourceId,
        Guid currentUserId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves recommended resources within a community filtered by resource type.
    /// Ranked by weighted engagement score: (LikesCount � 2) + ViewsCount.
    /// </summary>
    /// <param name="communityId">The identifier of the community.</param>
  
    /// <param name="query">Pagination parameters for the recommendation list.</param>
    /// <param name="currentUserId">
    /// The identifier of the current user used for personalization such as like status.
    /// </param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a paginated list of recommended <see cref="ResourceDto"/>.
    /// </returns>
    Task<Result<PaginatedResult<ResourceDto>>> GetRecommendedAsync(
        Guid communityId,
        
        ResourceQueryParams query,
        Guid currentUserId,
        CancellationToken ct = default);

    /// <summary>
    /// Increments the view count for a specific resource.
    /// Uses ExecuteUpdateAsync internally � no SaveChanges required by the caller.
    /// </summary>
    /// <param name="resourceId">The identifier of the resource being viewed.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A <see cref="Result"/> indicating whether the operation was successful.</returns>
    Task<Result> AddViewAsync(
        Guid resourceId,
        CancellationToken ct = default);

    /// <summary>
    /// Toggles the like status for a resource by the specified user.
    /// If the user has already liked the resource, the like will be removed.
    /// Otherwise, a new like will be added. Membership is required.
    /// </summary>
    /// <param name="resourceId">The identifier of the resource.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A <see cref="Result"/> indicating whether the like/unlike operation succeeded.</returns>
    Task<Result> ToggleLikeAsync(
        Guid resourceId,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Soft-deletes a resource by setting IsDeleted to true.
    /// Allowed for the original uploader, a community Owner, or a Moderator.
    /// </summary>
    /// <param name="resourceId">The identifier of the resource to delete.</param>
    /// <param name="userId">
    /// The identifier of the user requesting the deletion.
    /// Used to verify ownership or permissions.
    /// </param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A <see cref="Result"/> indicating whether the resource was successfully deleted.</returns>
    Task<Result> DeleteResourceAsync(
        Guid resourceId,
        Guid userId,
        CancellationToken ct = default);
}
