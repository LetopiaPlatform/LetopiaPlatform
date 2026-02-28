using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Project.Request;
using LetopiaPlatform.Core.DTOs.Project.Response;

namespace LetopiaPlatform.Core.Interfaces;
/// <summary>
/// 
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// 
    /// </summary>

    Task<Result<Guid>> CreateAsync(CreateProjectRequestDto request, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    Task<Result<ProjectDetailsResponseDto>> GetDetailsAsync(Guid id, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>

    Task<Result<PaginatedResult<ProjectDiscoverResponseDto>>> GetDiscoverAsync(ProjectFilterDto filter, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>

    Task<Result<string>> UpdateAsync(Guid id, UpdateProjectRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// 
    /// </summary>

    Task<Result<string>> DeleteAsync(Guid id, CancellationToken ct = default);
}
