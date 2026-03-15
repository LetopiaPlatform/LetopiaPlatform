using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Interfaces.Repositories;

/// <summary>
/// Repository for managing <see cref="Tag"/> entities across all taggable targets.
/// </summary>
public interface ITagRepository : IGenericRepository<Tag>
{
    /// <summary>
    /// Returns all tags for a specific target entity.
    /// </summary>
    /// <param name="targetType">The type of the tagged entity (Post, Resource, …).</param>
    /// <param name="targetId">The PK of the tagged entity.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    Task<IReadOnlyList<Tag>> GetByTargetAsync(
        TagTarget targetType,
        Guid targetId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all tags for multiple target IDs of the same type in one query.
    /// Used to avoid N+1 when mapping a page of resources to DTOs.
    /// </summary>
    /// <param name="targetType">The type of the tagged entities.</param>
    /// <param name="targetIds">The PKs of the tagged entities.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    Task<ILookup<Guid, Tag>> GetByTargetsAsync(
        TagTarget targetType,
        IEnumerable<Guid> targetIds,
        CancellationToken ct = default);

    /// <summary>
    /// Replaces all tags for a target entity with the provided list.
    /// Deletes existing tags and inserts the new ones within the same call.
    /// SaveChanges is handled by the caller via <c>IUnitOfWork</c>.
    /// </summary>
    /// <param name="targetType">The type of the tagged entity.</param>
    /// <param name="targetId">The PK of the tagged entity.</param>
    /// <param name="tagNames">New normalized tag names to set.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    Task ReplaceTagsAsync(
        TagTarget targetType,
        Guid targetId,
        IEnumerable<string> tagNames,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes all tags for a target entity.
    /// Called when the entity itself is hard-deleted.
    /// SaveChanges is handled by the caller via <c>IUnitOfWork</c>.
    /// </summary>
    /// <param name="targetType">The type of the tagged entity.</param>
    /// <param name="targetId">The PK of the tagged entity.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    Task DeleteByTargetAsync(
        TagTarget targetType,
        Guid targetId,
        CancellationToken ct = default);
}
