using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.CommunityTask.Request;
using LetopiaPlatform.Core.DTOs.CommunityTask.Response;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;

namespace LetopiaPlatform.Infrastructure.Services;
public class CommunityTaskService : ICommunityTaskService
{
    private readonly ICommunityTaskRepository _taskRepo;
    private readonly ICommunityRepository _communityRepo;

    public CommunityTaskService(ICommunityTaskRepository taskRepo, ICommunityRepository communityRepo)
    {
        _taskRepo = taskRepo;
        _communityRepo = communityRepo;
    }

    public async Task<Result<List<CommunityTaskResponseDto>>> GetTasksAsync(Guid communityId, Guid userId, CommunityTaskStatus status, CancellationToken ct = default)
    {
        var isMember = await _communityRepo.IsMemberAsync(communityId, userId, ct);
        if (!isMember) return Result<List<CommunityTaskResponseDto>>.Failure("You are not a member of this community", 403);

        var tasks = await _taskRepo.GetFilteredTasksAsync(communityId, userId, status, ct);
        return Result<List<CommunityTaskResponseDto>>.Success(tasks.Select(t => MapToDto(t, userId)).ToList());
    }

    public async Task<Result<CommunityProgressDto>> GetTodayProgressAsync(Guid communityId, Guid userId, CancellationToken ct = default)
    {


        var isMember = await _communityRepo.IsMemberAsync(communityId, userId, ct);

        if (!isMember)
            return Result<CommunityProgressDto>.Failure("Access Denied: You are not a member of this community", 403);


        var (total, completed) = await _taskRepo.GetProgressCountsAsync(communityId, userId, ct);
        return Result<CommunityProgressDto>.Success(CalculateProgress(total, completed));
    }

    public async Task<Result<Guid>> CreateAsync(Guid communityId, Guid userId, CreateTaskRequestDto request, CancellationToken ct = default)
    {
        var community = await _communityRepo.GetByIdAsync(communityId, ct);
        if (community == null) return Result<Guid>.Failure("Community not found", 404);
        if (community.CreatedBy != userId) return Result<Guid>.Failure("Only owner can create tasks", 403);

        if (await _taskRepo.IsTitleExistsAsync(communityId, request.Title, null, ct))
            return Result<Guid>.Failure("A task with this title already exists", 400);

        var task = new CommunityTask
        {
            Title = request.Title,
            Description = request.Description,
            Deadline = request.Deadline,
            CategoryId = request.CategoryId,
            CommunityId = communityId
        };

        await _taskRepo.AddAsync(task);
        return Result<Guid>.Success(task.Id);
    }

    public async Task<Result<string>> UpdateAsync(Guid id, Guid userId, UpdateTaskRequestDto request, CancellationToken ct = default)
    {
        var task = await _taskRepo.GetByIdWithCommunityAsync(id, ct);
        if (task == null) return Result<string>.Failure("Task not found", 404);
        if (task.Community.CreatedBy != userId) return Result<string>.Failure("Unauthorized to update", 403);

        if (await _taskRepo.IsTitleExistsAsync(task.CommunityId, request.Title, id, ct))
            return Result<string>.Failure("Title already exists", 400);

        task.Title = request.Title;
        task.Description = request.Description;
        task.Deadline = request.Deadline;
        task.CategoryId = request.CategoryId;

        await _taskRepo.UpdateAsync(task);
        return Result<string>.Success("UpdateOperationIsSuccessfully");
    }

    public async Task<Result<string>> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var task = await _taskRepo.GetByIdWithCommunityAsync(id, ct);
        if (task == null) return Result<string>.Failure("Task not found", 404);
        if (task.Community.CreatedBy != userId) return Result<string>.Failure("Unauthorized to delete", 403);

        await _taskRepo.DeleteAsync(task);
        return Result<string>.Success("DeleteOperationIsSuccessfully");
    }

    public async Task<Result<bool>> ToggleCompletionAsync(Guid taskId, Guid userId, CancellationToken ct = default)
    {

        var task = await _taskRepo.GetByIdAsync(taskId);
        if (task == null) return Result<bool>.Failure("Task not found", 404);

        if (!await _communityRepo.IsMemberAsync(task.CommunityId, userId, ct))
            return Result<bool>.Failure("You are not a member of this community", 403);

        if (DateTime.UtcNow > task.Deadline)
        {
            return Result<bool>.Failure("Cannot change status. The deadline for this task has passed.", 400);
        }

        var progress = await _taskRepo.GetTaskProgressAsync(taskId, userId, ct);

        if (progress == null)
        {
            progress = new UserTaskProgress
            {
                TaskId = taskId,
                UserId = userId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            };
            await _taskRepo.AddTaskProgressAsync(progress, ct);
        }
        else
        {
            progress.IsCompleted = !progress.IsCompleted;
            progress.CompletedAt = progress.IsCompleted ? DateTime.UtcNow : null;
        }

        await _taskRepo.SaveChangesAsync(ct);
        return Result<bool>.Success(progress.IsCompleted);
    }

    // ── Helpers ──────────────────────────────────────────────

    private static CommunityProgressDto CalculateProgress(int total, int completed)
    {
        double percentage = total == 0 ? 0 : Math.Round((double)completed / total * 100, 0);
        return new CommunityProgressDto(total, completed, percentage, $"{completed} of {total} tasks completed");
    }

    private static CommunityTaskResponseDto MapToDto(CommunityTask t, Guid userId)
    {
        return new CommunityTaskResponseDto(
            t.Id, t.Title, t.Description, t.Deadline,
            t.Category?.Name, t.Category?.ColorHex ?? "#6366f1", t.Category?.IconKey,
            t.UserProgresses.Any(up => up.UserId == userId && up.IsCompleted)
        );
    }
}
