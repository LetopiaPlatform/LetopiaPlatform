using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Data access operations for the shared Category entity.
/// Does NOT manage persistence — use IUnitOfWork for SaveChanges.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Retrieves a category by its unique identifier.
    /// </summary>
    /// <param name="id">The category ID to retrieve.</param>
    /// <param name="ct">Cancellation token for async operations.</param>
    /// <returns>The category if found; otherwise null.</returns>
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Retrieves a category by its slug and type.
    /// </summary>
    /// <param name="slug">The category slug (URL-friendly identifier).</param>
    /// <param name="type">The category type enum value.</param>
    /// <param name="ct">Cancellation token for async operations.</param>
    /// <returns>The category if found; otherwise null.</returns>
    Task<Category?> GetBySlugAsync(string slug, CategoryType type, CancellationToken ct = default);
    
    /// <summary>
    /// Checks if a slug already exists for the given category type.
    /// </summary>
    /// <param name="slug">The slug to check.</param>
    /// <param name="type">The category type to filter by.</param>
    /// <param name="excludeId">The category ID to exclude from the check (for updates). Optional.</param>
    /// <param name="ct">Cancellation token for async operations.</param>
    /// <returns>True if the slug exists for the given type; otherwise false.</returns>
    Task<bool> SlugExistsAsync(string slug, CategoryType type, Guid? excludeId = null, CancellationToken ct = default);
    
    /// <summary>
    /// Retrieves all categories of the specified type, ordered by default sorting.
    /// </summary>
    /// <param name="type">The category type to filter by.</param>
    /// <param name="ct">Cancellation token for async operations.</param>
    /// <returns>An enumerable collection of all categories matching the specified type.</returns>
    Task<IEnumerable<Category>> GetByTypeOrderedAsync(CategoryType type, CancellationToken ct = default);
    
    /// <summary>
    /// Adds a new category to the repository.
    /// </summary>
    /// <param name="category">The category entity to add.</param>
    void Add(Category category);
    
    /// <summary>
    /// Updates an existing category in the repository.
    /// </summary>
    /// <param name="category">The category entity with updated values.</param>
    void Update(Category category);
    
    /// <summary>
    /// Deletes a category from the repository.
    /// </summary>
    /// <param name="category">The category entity to delete.</param>
    void Delete(Category category);

    /// <summary>
    /// Checks if a category has any dependent communities or projects.
    /// </summary>
    Task<bool> HasDependentsAsync(Guid categoryId, CancellationToken ct = default);
}
