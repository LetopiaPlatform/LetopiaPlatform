using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;
/// <summary>
/// Concrete implementation of <see cref="IPostRepository"/>.
/// Inherits basic CRUD from <see cref="GenericRepository{T}"/>.
/// </summary>
public class PostRepository : GenericRepository<Post>, IPostRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="PostRepository"/>.
    /// </summary>
    /// <param name="context">The database context used for entity operations.</param>
    public PostRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<int> GetCommentCountAsync(Guid postId)
    {
        return await _context.Set<Comment>()
                             .CountAsync(c => c.PostId == postId && !c.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<int> GetReactionCountAsync(Guid postId, ReactionType? type = null)
    {
        var query = _context.Set<Reaction>()
                            .Where(r => r.TargetType == "Post" && r.TargetId == postId);

        if (type.HasValue)
            query = query.Where(r => r.ReactionType == type.Value);

        return await query.CountAsync();
    }
}
