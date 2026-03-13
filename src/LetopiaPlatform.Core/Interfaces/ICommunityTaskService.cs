using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.CommunityTask.Request;
using LetopiaPlatform.Core.DTOs.CommunityTask.Response;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Interfaces;
/// <summary>
/// 
/// </summary>
public interface ICommunityTaskService
{
    /// <summary>
    /// 
    /// </summary>
    Task<Result<List<CommunityTaskResponseDto>>> GetTasksAsync(Guid communityId, Guid userId, CommunityTaskStatus status, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    Task<Result<CommunityProgressDto>> GetTodayProgressAsync(Guid communityId, Guid userId, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    Task<Result<Guid>> CreateAsync(Guid communityId, Guid userId, CreateTaskRequestDto request, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    Task<Result<string>> UpdateAsync(Guid id, Guid userId, UpdateTaskRequestDto request, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    Task<Result<string>> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    Task<Result<bool>> ToggleCompletionAsync(Guid taskId, Guid userId, CancellationToken ct = default);
}
