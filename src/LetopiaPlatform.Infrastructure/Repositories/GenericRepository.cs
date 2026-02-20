
using System.Linq.Expressions;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<T> AddAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params string[] includes)
    {
        IQueryable<T> query = ApplyIncludes(_dbSet.AsQueryable(), includes);
        return await query.Where(predicate).ToListAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync(params string[] includes)
    {
        var query = ApplyIncludes(_dbSet.AsQueryable(), includes);
        return await query.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id, params string[] includes)
    {
        var query = ApplyIncludes(_dbSet.AsQueryable(), includes);
        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
    }

    public async Task UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaginatedResult<T>> GetPagedAsync(
        PaginatedQuery query,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        params string[] includes)
    {
        IQueryable<T> queryable = _dbSet.AsQueryable();

        queryable = ApplyIncludes(queryable, includes);

        if (predicate is not null)
        {
            queryable = queryable.Where(predicate);
        }

        var totalItems = await queryable.CountAsync();

        if (orderBy is not null)
        {
            queryable = ascending
                ? queryable.OrderBy(orderBy)
                : queryable.OrderByDescending(orderBy);
        }

        var items = await queryable
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return PaginatedResult<T>.Create(items, totalItems, query.Page, query.PageSize);
    }

    private static IQueryable<T> ApplyIncludes(IQueryable<T> query, string[] includes)
    {
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return query;
    }
}
