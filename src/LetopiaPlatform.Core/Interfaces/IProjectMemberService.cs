using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Project.Response;
using LetopiaPlatform.Core.DTOs.ProjectMember.Request;
using LetopiaPlatform.Core.DTOs.ProjectMember.Response;

namespace LetopiaPlatform.Core.Interfaces;
/// <summary>
/// 
/// </summary>
public interface IProjectMemberService
{
    /// <summary>
    /// 
    /// </summary>
    Task<Result<List<ProjectDiscoverResponseDto>>> GetMyProjectsAsync(Guid userId, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    Task<Result<ProjectMembersListDto>> GetProjectMembersAsync(Guid projectId, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    Task<Result<bool>> LeaveProjectAsync(ProjectMemberRequestDto request, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    Task<Result<bool>> JoinProjectAsync(ProjectMemberRequestDto request, CancellationToken ct = default);
}
