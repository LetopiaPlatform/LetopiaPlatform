using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.CommunityResourse;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;

public class ResourceRepository : GenericRepository<CommunityResource>, IResourceRepository
{
    private readonly ApplicationDbContext _context;

    public ResourceRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // ── Community feed ────────────────────────────────────────────────────────

    public async Task<PaginatedResult<CommunityResource>> GetResourcesByCommunityAsync(
        Guid communityId, ResourceQueryParams? query, CancellationToken ct = default)
    {
        // Derive safe pagination values — fall back to defaults when query is null
        var page = query?.Page ?? 1;
        var pageSize = query?.PageSize ?? 10;

        var q = _context.CommunityResources
            .Where(r => r.CommunityId == communityId); // IsDeleted handled by global query filter

        // Only apply filters when query is provided
        if (query?.Type is not null)
            q = q.Where(r => r.Type == query.Type.Value);

        if (!string.IsNullOrWhiteSpace(query?.Tag))
            q = q.Where(r => _context.Tags
                .Any(t => t.TargetType == TagTarget.Resource
                       && t.TargetId == r.Id
                       && t.TagName == query.Tag));

        var totalItems = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PaginatedResult<CommunityResource>.Create(items, totalItems, page, pageSize);
    }

    // ── Single resource with full navigation ──────────────────────────────────

    public async Task<CommunityResource?> GetByIdWithDetailsAsync(
        Guid resourceId, CancellationToken ct = default)
        => await _context.CommunityResources
           
            .Include(r => r.Likes)
            .FirstOrDefaultAsync(r => r.Id == resourceId, ct);

    // ── Recommended ───────────────────────────────────────────────────────────

    public async Task<PaginatedResult<CommunityResource>> GetRecommendedAsync(
        Guid communityId, ResourceType type, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _context.CommunityResources
         
            .Where(r => r.CommunityId == communityId && r.Type == type)
            .OrderByDescending(r => r.LikesCount * 2 + r.ViewsCount);

        var totalItems = await q.CountAsync(ct);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PaginatedResult<CommunityResource>.Create(items, totalItems, page, pageSize);
    }

    // ── Duplicate check ───────────────────────────────────────────────────────

    public async Task<CommunityResource?> GetByUrlAsync(
        string url, CancellationToken ct = default)
        => await _context.CommunityResources
            .FirstOrDefaultAsync(r => r.Url == url, ct);

    // ── Engagement ────────────────────────────────────────────────────────────

    public async Task<bool> IsLikedByUserAsync(
        Guid resourceId, Guid userId, CancellationToken ct = default)
        => await _context.ResourceLikes
            .AnyAsync(l => l.ResourceId == resourceId && l.UserId == userId, ct);

    public async Task IncrementViewsAsync(
        Guid resourceId, CancellationToken ct = default)
        => await _context.CommunityResources
            .Where(r => r.Id == resourceId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.ViewsCount, r => r.ViewsCount + 1)
                .SetProperty(r => r.UpdatedAt, DateTime.UtcNow),
                ct);

    public async Task AddLikeAsync(
        Guid resourceId, Guid userId, CancellationToken ct = default)
    {
        _context.ResourceLikes.Add(new ResourceLike
        {
            Id = Guid.NewGuid(),
            ResourceId = resourceId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
        });

        await _context.CommunityResources
            .Where(r => r.Id == resourceId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.LikesCount, r => r.LikesCount + 1),
                ct);
    }

    public async Task RemoveLikeAsync(
        Guid resourceId, Guid userId, CancellationToken ct = default)
    {
        await _context.ResourceLikes
            .Where(l => l.ResourceId == resourceId && l.UserId == userId)
            .ExecuteDeleteAsync(ct);

        await _context.CommunityResources
            .Where(r => r.Id == resourceId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.LikesCount, r => r.LikesCount - 1),
                ct);
    }
}
