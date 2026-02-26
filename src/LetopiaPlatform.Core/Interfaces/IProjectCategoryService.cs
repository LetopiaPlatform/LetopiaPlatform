using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.ProjectCategory.Request;
using LetopiaPlatform.Core.DTOs.ProjectCategory.Response;

namespace LetopiaPlatform.Core.Interfaces;
/// <summary>
/// projectcategoryservice
/// </summary>
public interface IProjectCategoryService
{
    /// <summary>
    /// Determine if a category exists by slug and return its details along with non-full projects in that category
    /// </summary>
    Task<Result<IEnumerable<CategoryResponse>>> GetAllOrderedAsync(CancellationToken ct = default);

    /// <summary>
    ///Determinre if a category exists by slug and return its details along with non-full projects in that category
    /// </summary>
    Task<Result<CategoryResponse>> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Detetmine if a category exists by slug and return its details along with non-full projects in that category
    /// </summary>
    Task<Result<Dictionary<Guid, int>>> GetCategoryStatsAsync(CancellationToken ct = default);

    /// <summary>
    /// Determine if a category exists by slug and return its details along with non-full projects in that category
    /// </summary>
    Task<Result<bool>> DeleteCategoryAsync(Guid id, CancellationToken ct = default);
    /// <summary>
    /// Determine if a category exists by slug and return its details along with non-full projects in that category
    /// </summary>
    Task<Result<Guid>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken ct = default);

    /// <summary>
    /// 
    /// </summary>

    Task<Result<bool>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default);
}
