using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;
public class ProjectCategoryRepository : GenericRepository<ProjectCategory>, IProjectCategoryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<ProjectCategory> _projectCategories;

    public ProjectCategoryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;

        _projectCategories = _context.Set<ProjectCategory>();
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        return await _projectCategories.AnyAsync(
            pc => pc.Slug == slug && (excludeId == null || pc.Id != excludeId.Value),
            ct);
    }

    public async Task<IEnumerable<ProjectCategory>> GetOrderedCategoriesAsync(CancellationToken ct = default)
    {
        return await _projectCategories
            .OrderBy(pc => pc.DisplayOrder)
            .ThenBy(pc => pc.Name)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<Guid, int>> GetCategoryProjectCountsAsync(CancellationToken ct = default)
    {
        // استخدام GroupBy مباشرة على جدول المشاريع لأفضل أداء SQL
        return await _context.Projects
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, ct);
    }

    public async Task<bool> HasProjectsAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await _context.Projects.AnyAsync(p => p.CategoryId == categoryId, ct);
    }

    public async Task<ProjectCategory?> GetCategoryWithProjectsAsync(string slug, CancellationToken ct = default)
    {
        return await _projectCategories
            .Include(pc => pc.Projects.Where(p => !p.IsFull))
            .FirstOrDefaultAsync(pc => pc.Slug == slug, ct);
    }
}
