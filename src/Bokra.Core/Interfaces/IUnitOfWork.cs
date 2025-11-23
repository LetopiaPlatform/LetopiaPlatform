using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bokra.Core.Interfaces
{
    public interface IUnitOfWork<TContext> : IAsyncDisposable, IDisposable
    {

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

 
        Task BeginTransactionAsync();

        Task CommitAsync();


        Task RollbackAsync();
    }
}
