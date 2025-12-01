
using System.Linq.Expressions;

using Bokra.Core.Interfaces;
using Bokra.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bokra.Infrastructure.Repositories
{
    internal class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        private IQueryable<T> IncludeProperties(IQueryable<T> query, string[] includes)
        {
            if (includes != null)
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }
            return query;
        }

        
        public async Task<T> AddAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _dbSet.AddAsync(entity);
            return entity;
        }

        //  Count
        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        // Delete
        public Task DeleteAsync(T entity)
        {
            if (entity == null)
                throw new KeyNotFoundException("Entity is null.");

            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        // Find (database-side filtering using Expression)
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params string[] includes)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var query = IncludeProperties(_dbSet.AsQueryable(), includes);
            return await query.Where(predicate).ToListAsync();
        }

        //  Get All
        public async Task<IEnumerable<T>> GetAllAsync(params string[] includes)
        {
            var query = IncludeProperties(_dbSet.AsQueryable(), includes);
            return await query.ToListAsync();
        }

        //  Get By Id (Guid PK)
        public async Task<T> GetByIdAsync(Guid id, params string[] includes)
        {
            var query = IncludeProperties(_dbSet.AsQueryable(), includes);
            return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        }

        //  Update
        public Task UpdateAsync(T entity)
        {
            if (entity == null)
                throw new KeyNotFoundException("Entity is null.");

            _dbSet.Update(entity);
            return Task.CompletedTask;
        }
    }
}
