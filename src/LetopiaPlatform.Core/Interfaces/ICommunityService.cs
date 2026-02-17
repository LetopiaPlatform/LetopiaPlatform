using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Community;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Manages community lifecycle, membership, and role operations.
/// Throws domain exceptions (NotFoundException, ConflictException, etc.) for error cases which are caught by ExceptionMiddleware and mapped to HTTP status codes.
/// </summary>
public interface ICommunityService
{
    /// <summary>
    /// Create a new community with default channels (Annoncements + General) and assign the creator as Owner.
    /// </summary>
    /// <param name="request">The request object containing community details.</param>
    /// <param name="userId">The ID of the user creating the community.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The details of the created community.</returns>
    Task<CommunityDetailDto> CreateAsync(
        CreateCommunityRequest request,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Lists communities with optional filtering, searching, and sorting.
    /// </summary>
    /// <param name="query">The pagination and filtering query parameters.</param>
    /// <param name="category">The category to filter communities by.</param>
    /// <param name="search">The search term to filter communities by name or description.</param>
    /// <param name="sortBy">The field to sort the communities by.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A paginated list of community summaries.</returns>
    Task<PaginatedResult<CommunitySummaryDto>> ListAsync(
        PaginatedQuery query,
        string? category = null,
        string? search = null,
        string? sortBy = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets full community details by slug, including list and membership context.
    /// </summary>
    /// <param name="slug">The slug of the community.</param>
    /// <param name="currentUserId">The ID of the current user, if available.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The details of the community.</returns>
    Task<CommunityDetailDto> GetBySlugAsync(
        string slug,
        Guid? currentUserId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Update community settings. Only Owner or Moderator can update.
    /// </summary>
    /// <param name="communityId">The ID of the community to update.</param>
    /// <param name="request">The request object containing updated community details.</param>
    /// <param name="userId">The ID of the user performing the update.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated community details.</returns>
    Task<CommunityDetailDto> UpdateAsync(
        Guid communityId,
        UpdateCommunityRequest request,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Joins a community. Throws ConflictExcpetion if already a member.
    /// </summary>
    /// <param name="communityId">The ID of the community to join.</param>
    /// <param name="userId">The ID of the user joining the community.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task JoinAsync(Guid communityId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Leaves a community. Owner cannot leave (must transfer ownership first).
    /// </summary>
    /// <param name="communityId">The ID of the community to leave.</param>
    /// <param name="userId">The ID of the user leaving the community.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LeaveAsync(Guid communityId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Lists community members with pagination. Only visible to members of the community.
    /// </summary>
    /// <param name="communityId">The ID of the community.</param>
    /// <param name="query">The pagination query parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A paginated list of community members.</returns>
    Task<PaginatedResult<MemberDto>> GetMembersAsync(
        Guid communityId,
        PaginatedQuery query,
        CancellationToken ct = default);

    /// <summary>
    /// Changes a member's role. Only the Owner can do this.
    /// Transferring ownership is atomic: new owner gets Owner, old owner becomes Moderator.
    /// </summary>
    /// <param name="communityId">The ID of the community.</param>
    /// <param name="targetUserId">The ID of the user whose role is being changed.</param>
    /// <param name="request">The request object containing the new role details.</param>
    /// <param name="callerUserId">The ID of the user performing the role change.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ChangeRoleAsync(
        Guid communityId,
        Guid targetUserId,
        ChangeRoleRequest request,
        Guid callerUserId,
        CancellationToken ct = default);
}