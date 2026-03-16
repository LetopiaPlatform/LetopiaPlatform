using LetopiaPlatform.Core.Entities.Identity;

namespace LetopiaPlatform.Core.Interfaces.Repositories;

public interface IUserRefreshTokenRepository : IGenericRepository<UserRefreshToken>
{
    /// <summary>
    /// Purges expired or invalidated refresh tokens for a specific user from the database to maintain optimal table size.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose tokens should be cleaned up.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteExpiredTokensAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Provides an <see cref="IQueryable{UserRefreshToken}"/> of the tokens table with change tracking enabled for advanced queries and updates.
    /// </summary>
    /// <returns>An IQueryable represent the refresh tokens table with tracking.</returns>
    IQueryable<UserRefreshToken> GetTableAsTracking();


}
