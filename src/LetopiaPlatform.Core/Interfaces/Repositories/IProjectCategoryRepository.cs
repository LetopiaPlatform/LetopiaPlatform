using LetopiaPlatform.Core.Entities;
namespace LetopiaPlatform.Core.Interfaces.Repositories;
public interface IProjectCategoryRepository : IGenericRepository<ProjectCategory>
{
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
    Task<IEnumerable<ProjectCategory>> GetOrderedCategoriesAsync(CancellationToken ct = default);
    Task<Dictionary<Guid, int>> GetCategoryProjectCountsAsync(CancellationToken ct = default);
    Task<bool> HasProjectsAsync(Guid categoryId, CancellationToken ct = default);
    Task<ProjectCategory?> GetCategoryWithProjectsAsync(string slug, CancellationToken ct = default);
}
