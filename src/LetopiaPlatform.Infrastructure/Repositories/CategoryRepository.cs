using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;

internal sealed class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Category?> GetBySlugAsync(string slug, CategoryType type, CancellationToken ct = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug && c.Type == type, ct);
    }

    public async Task<bool> SlugExistsAsync(
        string slug, CategoryType type,
        Guid? excludeId = null, CancellationToken ct = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Slug == slug
                        && c.Type == type
                        && (excludeId == null || c.Id != excludeId.Value), ct);
    }

    public async Task<IEnumerable<Category>> GetByTypeOrderedAsync(
        CategoryType type, CancellationToken ct = default)
    {
        return await _dbContext.Categories
            .Where(c => c.Type == type)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public void Add(Category category) => _dbContext.Categories.Add(category);
    public void Update(Category category) => _dbContext.Categories.Update(category);
    public void Delete(Category category) => _dbContext.Categories.Remove(category);
}