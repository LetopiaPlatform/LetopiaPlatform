using System.Linq.Expressions;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces.Repositories;
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

    public PostRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // ── Comment / Reaction counts ─────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<int> GetCommentCountAsync(Guid postId, CancellationToken ct = default)
        => await _context.Set<Comment>()
                         .CountAsync(c => c.PostId == postId && !c.IsDeleted, ct);

    /// <inheritdoc/>
    public async Task<int> GetReactionCountAsync(
        Guid postId, ReactionType? type = null, CancellationToken ct = default)
    {
        var query = _context.Set<Reaction>()
                            .Where(r => r.TargetType == TargetType.Post && r.TargetId == postId);

        if (type is not null)
            query = query.Where(r => r.ReactionType == type);

        return await query.CountAsync(ct);
    }

    // ── Paged list ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<PaginatedResult<Post>> GetPagedAsync(
        PaginatedQuery query,
        Guid communityId,
        Guid channelId,
        string? search,
        string? sortBy,
        CancellationToken ct = default)
    {
        var hasSearch = !string.IsNullOrWhiteSpace(search);
        var keyword = hasSearch ? $"%{search!.Trim()}%" : null;

        var q = _context.Posts
            .Include(p => p.Author)
            .Where(p =>
                p.CommunityId == communityId &&
                p.ChannelId == channelId &&
                !p.IsDeleted &&
                !p.IsPinned &&           // pinned posts are returned separately via GetPinnedAsync
                (!hasSearch ||
                 EF.Functions.Like(p.Title!, keyword!) ||
                 EF.Functions.Like(p.Content!, keyword!)));

        q = sortBy?.ToLowerInvariant() switch
        {
            "upvotes" => q.OrderByDescending(p => p.Upvotes),
            "comments" => q.OrderByDescending(p => p.CommentCount),
            _ => q.OrderByDescending(p => p.CreatedAt),
        };

        var totalItems = await q.CountAsync(ct);

        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return PaginatedResult<Post>.Create(items, totalItems, query.Page, query.PageSize);
    }

    // ── Pinned posts ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Post>> GetPinnedAsync(
        Guid communityId,
        Guid channelId,
        CancellationToken ct = default)
    {
        return await _context.Posts
            .Include(p => p.Author)
                .ThenInclude(uc => uc.User)
            .Where(p =>
                p.CommunityId == communityId &&
                p.ChannelId == channelId &&
                p.IsPinned &&
                !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    // ── Get by author ─────────────────────────────────────────────────────────


   

}
