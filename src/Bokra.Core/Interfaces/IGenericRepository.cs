

using System.Linq.Expressions;


namespace Bokra.Core.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(params string[] includes);
        Task<T> GetByIdAsync(Guid id, params string[] includes);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params string[] includes);

        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<int> CountAsync();
    }
}
