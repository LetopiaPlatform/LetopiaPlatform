using System;
using System.Threading;
using System.Threading.Tasks;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Coordinates multiple repository operations within a single database transaction.
/// </summary>
/// <typeparam name="TContext">The database context type managed by this unit of work.</typeparam>
public interface IUnitOfWork<TContext> : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction, making all changes permanent.
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// Rolls back the current transaction, discarding all pending changes.
    /// </summary>
    Task RollbackAsync();
}
