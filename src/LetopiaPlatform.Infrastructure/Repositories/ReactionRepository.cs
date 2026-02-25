using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;
/// <summary>
/// Concrete implementation of <see cref="IReactionRepository"/>.
/// Inherits basic CRUD operations from <see cref="GenericRepository{T}"/>.
/// </summary>
public class ReactionRepository : GenericRepository<Reaction>, IReactionRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="ReactionRepository"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ReactionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Reaction>> GetReactionsByTargetAsync(string targetType, Guid targetId)
    {
        return await _context.Set<Reaction>()
                             .Where(r => r.TargetType == targetType && r.TargetId == targetId)
                             .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<int> GetReactionCountAsync(string targetType, Guid targetId, ReactionType? type = null)
    {
        var query = _context.Set<Reaction>()
                            .Where(r => r.TargetType == targetType && r.TargetId == targetId);

        if (type.HasValue)
            query = query.Where(r => r.ReactionType == type.Value);

        return await query.CountAsync();
    }

    /// <inheritdoc/>
    public async Task<Reaction?> GetUserReactionAsync(Guid userId, string targetType, Guid targetId)
    {
        return await _context.Set<Reaction>()
                             .FirstOrDefaultAsync(r =>
                                 r.UserId == userId &&
                                 r.TargetType == targetType &&
                                 r.TargetId == targetId);
    }
}
