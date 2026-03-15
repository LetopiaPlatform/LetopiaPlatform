using LetopiaPlatform.Core.Entities.Identity;

namespace LetopiaPlatform.Core.Interfaces.Repositories;

public interface IUserRefreshTokenRepository : IGenericRepository<UserRefreshToken>
{
    /// <summary>
    /// </summary>
    Task DeleteExpiredTokensAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// </summary>
    IQueryable<UserRefreshToken> GetTableAsTracking();

    /// <summary>
    /// </summary>
    Task<UserRefreshToken?> GetByHashAsync(string hash, Guid userId, CancellationToken ct = default);
}
