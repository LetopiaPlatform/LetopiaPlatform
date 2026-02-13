using System.Linq.Expressions;
using LetopiaPlatform.Core.Common;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Provides generic CRUD operations for entities.
/// </summary>
/// <typeparam name="T">The entity type this repository manages.</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Retrieves all entities, optionally including related navigation properties.
    /// </summary>
    /// <param name="includes">Navigation property names to eagerly load.</param>
    /// <returns>A collection of all entities.</returns>
    Task<IEnumerable<T>> GetAllAsync(params string[] includes);

    /// <summary>
    /// Retrieves a single entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="includes">Navigation property names to eagerly load.</param>
    /// <returns>The entity if found; otherwise <c>null</c>.</returns>
    Task<T?> GetByIdAsync(Guid id, params string[] includes);

    /// <summary>
    /// Retrieves entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A filter expression to apply.</param>
    /// <param name="includes">Navigation property names to eagerly load.</param>
    /// <returns>A collection of matching entities.</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params string[] includes);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The added entity with any generated values populated.</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Returns the total number of entities in the repository.
    /// </summary>
    /// <returns>The entity count.</returns>
    Task<int> CountAsync();

    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated subset of entities with optional filtering and eager loading.
    /// </summary>
    /// <param name="query">Pagination parameters (page number, page size).</param>
    /// <param name="predicate">Optional filter expression to apply.</param>
    /// <param name="orderBy">Optional sorting expression.</param>
    /// <param name="ascending">Sort direction. Default is <c>true</c> (ascending).</param>
    /// <param name="includes">Navigation property names to eagerly load.</param>
    /// <returns>A <see cref="PaginatedResult{T}"/> containing the paginated entities.</returns>
    Task<PaginatedResult<T>> GetPagedAsync(
        PaginatedQuery query,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        params string[] includes);
}