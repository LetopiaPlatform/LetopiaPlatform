using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Project.Request;
using LetopiaPlatform.Core.Entities;

namespace LetopiaPlatform.Core.Interfaces.Repositories;
/// <summary>
/// 
/// </summary>
public interface IProjectRepository : IGenericRepository<Project>
{
    /// <summary>
    /// 
    /// </summary>

    Task<PaginatedResult<Project>> GetFilteredProjectsAsync(
         ProjectFilterDto filter, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    Task<Project?> GetProjectWithDetailsAsync(Guid id, CancellationToken ct = default);

    Task<bool> IsTitleExistsInCategoryAsync(string title, Guid categoryId, CancellationToken ct = default);

}
