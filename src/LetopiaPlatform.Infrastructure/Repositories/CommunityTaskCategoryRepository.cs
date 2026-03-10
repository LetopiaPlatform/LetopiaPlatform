using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;
public class CommunityTaskCategoryRepository : GenericRepository<CommunityTaskCategory>, ICommunityTaskCategoryRepository
{

    private readonly ApplicationDbContext _context;
    private readonly DbSet<CommunityTaskCategory> _communityTaskCategories;

    public CommunityTaskCategoryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;

        _communityTaskCategories = _context.Set<CommunityTaskCategory>();
    }
    public async Task<List<CommunityTaskCategory>> GetCategoriesWithTasksAsync(Guid communityId, CancellationToken ct = default)
    {
        return await _communityTaskCategories
            .Where(c => c.CommunityId == communityId)
            .Include(c => c.Tasks)
                .ThenInclude(t => t.UserProgresses)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<CommunityTaskCategory?> GetByIdWithCommunityAsync(Guid id, CancellationToken ct = default)
    {
        return await _communityTaskCategories
            .Include(c => c.Community)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<bool> IsNameExistsAsync(Guid communityId, string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        return await _communityTaskCategories
        .AnyAsync(c => c.CommunityId == communityId
                    && (excludeId == null || c.Id != excludeId)
                    && c.Name == name, ct);
    }
}
