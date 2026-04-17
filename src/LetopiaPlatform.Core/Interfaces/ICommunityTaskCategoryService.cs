using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.CommunityTaskCategory.Request;
using LetopiaPlatform.Core.DTOs.CommunityTaskCategory.Response;

namespace LetopiaPlatform.Core.Interfaces;
/// <summary>
/// 
/// </summary>
public interface ICommunityTaskCategoryService
{
    /// <summary>
    /// 
    /// </summary>

    Task<Result<List<TaskCategoryResponseDto>>> GetCommunityCategoriesAsync(Guid communityId, Guid userId, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    Task<Result<Guid>> CreateAsync(Guid communityId, Guid userId, CreateCategoryRequestDto request, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    Task<Result<string>> UpdateAsync(Guid id, Guid userId, UpdateCategoryRequestDto request, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    Task<Result<string>> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    Task<Result<TaskCategoryResponseDto>> GetByIdAsync(Guid communityId, Guid id, Guid userId, CancellationToken ct = default);
}
