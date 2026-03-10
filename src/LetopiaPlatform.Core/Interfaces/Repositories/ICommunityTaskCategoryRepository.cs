using LetopiaPlatform.Core.Entities;

namespace LetopiaPlatform.Core.Interfaces.Repositories;
public interface ICommunityTaskCategoryRepository : IGenericRepository<CommunityTaskCategory>
{

    Task<List<CommunityTaskCategory>> GetCategoriesWithTasksAsync(Guid communityId, CancellationToken ct = default);
    Task<CommunityTaskCategory?> GetByIdWithCommunityAsync(Guid id, CancellationToken ct = default);
    Task<bool> IsNameExistsAsync(Guid communityId, string name, Guid? excludeId = null, CancellationToken ct = default);
}
