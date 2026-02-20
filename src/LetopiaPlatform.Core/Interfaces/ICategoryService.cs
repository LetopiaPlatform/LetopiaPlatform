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
    /// <returns>A result containing the created category DTO.</returns>
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="request">The category update request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result containing the updated category DTO.</returns>
    Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Deletes a category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result containing a boolean indicating success.</returns>
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Gets all categories of a specific type.
    /// </summary>
    /// <param name="type">The category type.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result containing a collection of category DTOs.</returns>
    Task<Result<IEnumerable<CategoryDto>>> GetByTypeAsync(CategoryType type, CancellationToken ct = default);
    
    /// <summary>
    /// Gets a category by its slug and type.
    /// </summary>
    /// <param name="slug">The category slug.</param>
    /// <param name="type">The category type.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result containing the category DTO.</returns>
    Task<Result<CategoryDto>> GetBySlugAsync(string slug, CategoryType type, CancellationToken ct = default);
}