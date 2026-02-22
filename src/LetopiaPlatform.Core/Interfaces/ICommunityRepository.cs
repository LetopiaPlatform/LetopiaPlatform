using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Community;
using LetopiaPlatform.Core.Entities;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Data access operations for communities, memberships, and channels.
/// Does NOT manage persistence — use IUnitOfWork for SaveChanges and transactions.
/// </summary>
public interface ICommunityRepository
{
    // Community
    /// <summary>
    /// Retrieves a community by its unique identifier.
    /// </summary>
    /// <param name="id">The community ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The community if found; otherwise null.</returns>
    Task<Community?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a community by its slug.
    /// </summary>
    /// <param name="slug">The community slug.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The community if found; otherwise null.</returns>
    Task<Community?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a slug exists for any community.
    /// </summary>
    /// <param name="slug">The slug to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the slug exists; otherwise false.</returns>
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Adds a new community to the repository.
    /// </summary>
    /// <param name="community">The community to add.</param>
    void AddCommunity(Community community);

    /// <summary>
    /// Lists communities with pagination and optional filtering.
    /// </summary>
    /// <param name="query">Pagination parameters.</param>
    /// <param name="category">Optional category filter.</param>
    /// <param name="search">Optional search term.</param>
    /// <param name="sortBy">Optional sort field.</param>
    /// <param name="currentUserId">The ID of the current user, if available, to include membership context in the results.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A paginated result of community summaries.</returns>
    Task<PaginatedResult<CommunitySummaryDto>> ListAsync(
        PaginatedQuery query,
        string? category = null,
        string? search = null,
        string? sortBy = null,
        Guid? currentUserId = null,
        CancellationToken ct = default);

    // Membership
    /// <summary>
    /// Retrieves the membership record for a specific user in a community.
    /// </summary>
    /// <param name="communityId">The community ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The membership record if found; otherwise null.</returns>
    Task<UserCommunity?> GetMembershipAsync(Guid communityId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a user is a member of a community.
    /// </summary>
    /// <param name="communityId">The community ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the user is a member; otherwise false.</returns>
    Task<bool> IsMemberAsync(Guid communityId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new member to a community.
    /// </summary>
    /// <param name="membership">The membership record to add.</param>
    void AddMember(UserCommunity membership);

    /// <summary>
    /// Removes a member from a community.
    /// </summary>
    /// <param name="membership">The membership record to remove.</param>
    void RemoveMember(UserCommunity membership);

    /// <summary>
    /// Increments the member count for a community.
    /// </summary>
    /// <param name="communityId">The community ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task IncrementMemberCountAsync(Guid communityId, CancellationToken ct = default);

    /// <summary>
    /// Decrements the member count for a community.
    /// </summary>
    /// <param name="communityId">The community ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DecrementMemberCountAsync(Guid communityId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves paginated members of a community.
    /// </summary>
    /// <param name="communityId">The community ID.</param>
    /// <param name="query">Pagination parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A paginated result of member information.</returns>
    Task<PaginatedResult<MemberDto>> GetMembersAsync(
        Guid communityId, PaginatedQuery query, CancellationToken ct = default);

    // Channels
    /// <summary>
    /// Adds multiple channels to a community.
    /// </summary>
    /// <param name="channels">The channels to add.</param>
    void AddChannels(IEnumerable<Channel> channels);

    /// <summary>
    /// Retrieves all channels for a community.
    /// </summary>
    /// <param name="communityId">The community ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of channels for the community.</returns>
    Task<List<Channel>> GetChannelsAsync(Guid communityId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all communities the user has joined, with membership context.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of joined community summaries.</returns>
    Task<List<JoinedCommunitySummaryDto>> GetJoinedCommunitiesAsync(
        Guid userId,
        CancellationToken ct = default);
}