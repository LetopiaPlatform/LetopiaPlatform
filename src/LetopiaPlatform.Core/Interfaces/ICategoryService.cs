using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Category;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Admin-managed CRUD operations for shared categories.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="request">The category creation request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The created category DTO.</returns>
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="request">The category update request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The updated category DTO.</returns>
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Deletes a category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Gets all categories of a specific type.
    /// </summary>
    /// <param name="type">The category type.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A collection of category DTOs.</returns>
    Task<IEnumerable<CategoryDto>> GetByTypeAsync(string type, CancellationToken ct = default);
    
    /// <summary>
    /// Gets a category by its slug and type.
    /// </summary>
    /// <param name="slug">The category slug.</param>
    /// <param name="type">The category type.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The category DTO.</returns>
    Task<CategoryDto> GetBySlugAsync(string slug, string type, CancellationToken ct = default);
}