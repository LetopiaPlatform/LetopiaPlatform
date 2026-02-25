using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;
/// <summary>
/// Concrete implementation of <see cref="ICommentRepository"/>.
/// Inherits basic CRUD operations from <see cref="GenericRepository{T}"/>.
/// </summary>
public class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="CommentRepository"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CommentRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<Comment>> GetCommentsByPostIdAsync(Guid postId, int page, int pageSize)
    {
        var query = _context.Set<Comment>()
                            .Where(c => c.PostId == postId && !c.IsDeleted);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PaginatedResult<Comment>.Create(items, total, page, pageSize);
    }

    /// <inheritdoc/>
    public async Task<int> GetReactionCountAsync(Guid commentId)
    {
        return await _context.Set<Reaction>()
                             .CountAsync(r => r.TargetType == "Comment" && r.TargetId == commentId);
    }
}
