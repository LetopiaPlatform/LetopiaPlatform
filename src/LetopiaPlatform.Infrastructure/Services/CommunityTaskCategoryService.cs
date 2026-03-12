using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.CommunityTaskCategory.Request;
using LetopiaPlatform.Core.DTOs.CommunityTaskCategory.Response;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;

namespace LetopiaPlatform.Infrastructure.Services;
public class CommunityTaskCategoryService : ICommunityTaskCategoryService
{
    private readonly ICommunityTaskCategoryRepository _categoryRepo;
    private readonly ICommunityRepository _communityRepo;

    public CommunityTaskCategoryService(
        ICommunityTaskCategoryRepository categoryRepo,
        ICommunityRepository communityRepo)
    {
        _categoryRepo = categoryRepo;
        _communityRepo = communityRepo;
    }

    public async Task<Result<List<TaskCategoryResponseDto>>> GetCommunityCategoriesAsync(Guid communityId, Guid userId, CancellationToken ct = default)
    {
        var categories = await _categoryRepo.GetCategoriesWithTasksAsync(communityId, ct);

        return Result<List<TaskCategoryResponseDto>>.Success(categories.Select(c => MapToDto(c, userId)).ToList());
    }

    public async Task<Result<Guid>> CreateAsync(Guid communityId, Guid userId, CreateCategoryRequestDto request, CancellationToken ct = default)
    {
        var community = await _communityRepo.GetByIdAsync(communityId, ct);
        if (community == null) return Result<Guid>.Failure("Community not found", 404);
        if (community.CreatedBy != userId) return Result<Guid>.Failure("Only community owner can create categories", 403);

        if (await _categoryRepo.IsNameExistsAsync(communityId, request.Name, null, ct))
            return Result<Guid>.Failure("Category name already exists", 400);

        var category = new CommunityTaskCategory
        {
            Name = request.Name,
            ColorHex = request.ColorHex,
            IconKey = request.IconKey,
            CommunityId = communityId
        };

        await _categoryRepo.AddAsync(category);
        return Result<Guid>.Success(category.Id);
    }

    public async Task<Result<string>> UpdateAsync(Guid id, Guid userId, UpdateCategoryRequestDto request, CancellationToken ct = default)
    {
        var category = await _categoryRepo.GetByIdWithCommunityAsync(id, ct);
        if (category is null) return Result<string>.Failure("Category not found", 404);

        if (category.Community.CreatedBy != userId)
            return Result<string>.Failure("Only community owner can update categories", 403);

        if (await _categoryRepo.IsNameExistsAsync(category.CommunityId, request.Name, id, ct))
            return Result<string>.Failure("Category name already exists", 400);

        category.Name = request.Name;
        category.ColorHex = request.ColorHex;
        category.IconKey = request.IconKey;

        await _categoryRepo.UpdateAsync(category);
        return Result<string>.Success("UpdateOperationIsSuccessfully");
    }

    public async Task<Result<string>> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var category = await _categoryRepo.GetByIdWithCommunityAsync(id, ct);
        if (category is null) return Result<string>.Failure("Category not found", 404);

        if (category.Community.CreatedBy != userId)
            return Result<string>.Failure("Only community owner can delete categories", 403);

        await _categoryRepo.DeleteAsync(category);
        return Result<string>.Success("DeleteOperationIsSuccessfully");
    }
    public async Task<Result<TaskCategoryResponseDto>> GetByIdAsync(Guid communityId, Guid id, Guid userId, CancellationToken ct = default)
    {
        var category = await _categoryRepo.GetByIdWithCommunityAsync(id, ct);

        if (category is null)
            return Result<TaskCategoryResponseDto>.Failure("Category not found", 404);

        if (category.CommunityId != communityId)
            return Result<TaskCategoryResponseDto>.Failure("This category does not belong to the specified community", 400);

        var response = MapToDto(category, userId);

        return Result<TaskCategoryResponseDto>.Success(response);
    }
    // ── Mapping Helpers ────────────────────────────────────────
    private static TaskCategoryResponseDto MapToDto(CommunityTaskCategory category, Guid userId)
    {
        int total = category.Tasks.Count;
        int completed = category.Tasks.Count(t => t.UserProgresses.Any(up => up.UserId == userId && up.IsCompleted));

        return new TaskCategoryResponseDto(
            category.Id, category.Name, category.ColorHex, category.IconKey,
            total, completed, total == 0 ? 0 : Math.Round((double)completed / total * 100, 0)
        );
    }
}
