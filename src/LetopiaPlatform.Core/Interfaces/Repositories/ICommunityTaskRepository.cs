using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Interfaces.Repositories;
public interface ICommunityTaskRepository : IGenericRepository<CommunityTask>
{
    Task<List<CommunityTask>> GetFilteredTasksAsync(Guid communityId, Guid userId, CommunityTaskStatus status, CancellationToken ct = default);
    Task<(int Total, int Completed)> GetProgressCountsAsync(Guid communityId, Guid userId, CancellationToken ct = default);
    Task<UserTaskProgress?> GetTaskProgressAsync(Guid taskId, Guid userId, CancellationToken ct = default);
    Task<bool> IsTitleExistsAsync(Guid communityId, string title, Guid? excludeId = null, CancellationToken ct = default);
    Task<CommunityTask?> GetByIdWithCommunityAsync(Guid id, CancellationToken ct = default);

    Task AddTaskProgressAsync(UserTaskProgress progress, CancellationToken ct = default);
}
