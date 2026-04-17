using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;
public class CommunityTaskRepository : GenericRepository<CommunityTask>, ICommunityTaskRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<CommunityTask> _tasks;

    public CommunityTaskRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
        _tasks = _context.Set<CommunityTask>();
    }

    public async Task<List<CommunityTask>> GetFilteredTasksAsync(Guid communityId, Guid userId, CommunityTaskStatus status, CancellationToken ct = default)
    {
        var query = _tasks
            .Include(t => t.Category)
            .Include(t => t.UserProgresses.Where(up => up.UserId == userId))
            .Where(t => t.CommunityId == communityId)
            .AsQueryable();

        query = status switch
        {
            CommunityTaskStatus.Completed => query.Where(t => t.UserProgresses.Any(up => up.UserId == userId && up.IsCompleted)),
            CommunityTaskStatus.Active => query.Where(t => !t.UserProgresses.Any(up => up.UserId == userId && up.IsCompleted) && t.Deadline >= DateTime.UtcNow),
            CommunityTaskStatus.Missed => query.Where(t => !t.UserProgresses.Any(up => up.UserId == userId && up.IsCompleted) && t.Deadline < DateTime.UtcNow),
            _ => query
        };

        return await query.AsNoTracking().ToListAsync(ct);
    }

    public async Task<(int Total, int Completed)> GetProgressCountsAsync(Guid communityId, Guid userId, CancellationToken ct = default)
    {
        var stats = await _tasks
            .Where(t => t.CommunityId == communityId)
            .Select(t => new { IsDone = t.UserProgresses.Any(up => up.UserId == userId && up.IsCompleted) })
            .ToListAsync(ct);

        return (stats.Count, stats.Count(s => s.IsDone));
    }

    public async Task<UserTaskProgress?> GetTaskProgressAsync(Guid taskId, Guid userId, CancellationToken ct = default)
    {
        return await _context.Set<UserTaskProgress>()
            .FirstOrDefaultAsync(p => p.TaskId == taskId && p.UserId == userId, ct);
    }

    public async Task AddTaskProgressAsync(UserTaskProgress progress, CancellationToken ct = default)
    {
        await _context.Set<UserTaskProgress>().AddAsync(progress, ct);
    }

    public async Task<bool> IsTitleExistsAsync(Guid communityId, string title, Guid? excludeId = null, CancellationToken ct = default)
    {
        var trimmedTitle = title.Trim();

        return await _tasks.AnyAsync(t => t.CommunityId == communityId
                                     && t.Title == trimmedTitle
                                     && (excludeId == null || t.Id != excludeId), ct);
    }

    public async Task<CommunityTask?> GetByIdWithCommunityAsync(Guid id, CancellationToken ct = default)
    {
        return await _tasks.Include(t => t.Community).FirstOrDefaultAsync(t => t.Id == id, ct);
    }
}
