using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;
public class UserRefreshTokenRepository : GenericRepository<UserRefreshToken>, IUserRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<UserRefreshToken> _refreshTokens;

    public UserRefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
        _refreshTokens = _context.Set<UserRefreshToken>();
    }

    public IQueryable<UserRefreshToken> GetTableAsTracking()
    {
        return _refreshTokens.AsTracking().AsQueryable();
    }

    public async Task DeleteExpiredTokensAsync(Guid userId, CancellationToken ct = default)
    {
        var tokensToDelete = await _refreshTokens
            .Where(t => t.UserId == userId &&
                       (t.ExpiryDate <= DateTime.UtcNow || t.IsUsed || t.IsRevoked))
            .ToListAsync(ct);

        if (tokensToDelete.Count > 0)
        {
            _refreshTokens.RemoveRange(tokensToDelete);
        }
    }


}
