using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Project.Request;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;
public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<Project> _projects;

    public ProjectRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
        _projects = _context.Set<Project>();
    }

    // ── Get Filtered Projects (For Discover) ────────────────────────────────
    public async Task<PaginatedResult<Project>> GetFilteredProjectsAsync(
         ProjectFilterDto filter, CancellationToken ct = default)
    {
        var query = _projects
            .Include(p => p.Owner)
            .Include(p => p.Category)
            .Include(p => p.Members)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(p => p.Title.Contains(filter.SearchTerm)
                                  || p.Description!.Contains(filter.SearchTerm));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return PaginatedResult<Project>.Create(items, totalCount, filter.Page, filter.PageSize);
    }

    // ── Get Project With Details (For Overview) ─────────────────────────────
    public async Task<Project?> GetProjectWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _projects
            .Include(p => p.Category)
            .Include(p => p.Owner)
            .Include(p => p.Milestones)
            .Include(p => p.Resources)
            .Include(p => p.Members)
                .ThenInclude(m => m.Member)
                .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<bool> IsTitleExistsInCategoryAsync(string title, Guid categoryId, CancellationToken ct = default)
    {
        return await _projects.AnyAsync(p =>
            p.Title == title &&
            p.CategoryId == categoryId,
            ct);
    }



}
