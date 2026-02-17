using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Community;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Repositories;

internal sealed class CommunityRepository : ICommunityRepository
{
    private readonly ApplicationDbContext _dbContext;
    public CommunityRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Community
    public async Task<Community?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Communities
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive, ct);
    }

    public async Task<Community?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _dbContext.Communities
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive, ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
    {
        return await _dbContext.Communities
            .AsNoTracking()
            .AnyAsync(c => c.Slug == slug && c.IsActive, ct);
    }

    public void AddCommunity(Community community)
    {
        _dbContext.Communities.Add(community);
    }

    public async Task<PaginatedResult<CommunitySummaryDto>> ListAsync(
        PaginatedQuery query,
        string? category = null,
        string? search = null,
        string? sortBy = null,
        CancellationToken ct = default)
    {
        var queryable = _dbContext.Communities
            .Where(c => c.IsActive)
            .AsNoTracking();
        
        if (!string.IsNullOrWhiteSpace(category))
        {
            queryable = queryable.Where(c => c.TopicCategory == category);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            queryable = queryable.Where(c => 
                c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                c.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        queryable = sortBy?.ToLowerInvariant() switch
        {
            "members" => queryable.OrderByDescending(c => c.MemberCount),
            "posts" => queryable.OrderByDescending(c => c.PostCount),
            "name" => queryable.OrderBy(c => c.Name),
            "oldest" => queryable.OrderBy(c => c.CreatedAt),
            _ => queryable.OrderByDescending(c => c.CreatedAt) // default to newest
        };

        var totalItems = await queryable.CountAsync(ct);

        var items = await queryable
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CommunitySummaryDto(
                Id: c.Id,
                Name: c.Name,
                Slug: c.Slug,
                Description: c.Description,
                TopicCategory: c.TopicCategory,
                IconUrl: c.IconUrl,
                MemberCount: c.MemberCount,
                PostCount: c.PostCount,
                IsPrivate: c.IsPrivate,
                CreatedAt: c.CreatedAt
            ))
            .ToListAsync(ct);

        return PaginatedResult<CommunitySummaryDto>.Create(items, totalItems, query.Page, query.PageSize);
    }

    public async Task<UserCommunity?> GetMembershipAsync(
        Guid communityId,
        Guid userId,
        CancellationToken ct = default)
    {
        return await _dbContext.UserCommunities
            .FirstOrDefaultAsync(uc => uc.CommunityId == communityId && uc.UserId == userId, ct);
    }

    public async Task<bool> IsMemberAsync(
        Guid communityId,
        Guid userId,
        CancellationToken ct = default)
    {
        return await _dbContext.UserCommunities
            .AnyAsync(uc => uc.CommunityId == communityId && uc.UserId == userId, ct);
    }

    public void AddMember(UserCommunity membership)
    {
        _dbContext.UserCommunities.Add(membership);
    }

    public void RemoveMember(UserCommunity membership)
    {
        _dbContext.UserCommunities.Remove(membership);
    }

    public async Task IncrementMemberCountAsync(Guid communityId, CancellationToken ct = default)
    {
        await _dbContext.Communities
            .Where(c => c.Id == communityId)
            .ExecuteUpdateAsync(s => 
                s.SetProperty(c => c.MemberCount, c => c.MemberCount + 1), ct);
    }

    public async Task DecrementMemberCountAsync(Guid communityId, CancellationToken ct = default)
    {
        await _dbContext.Communities
            .Where(c => c.Id == communityId)
            .ExecuteUpdateAsync(s => 
                s.SetProperty(c => c.MemberCount, c => c.MemberCount - 1), ct);
    }

    public async Task<PaginatedResult<MemberDto>> GetMembersAsync(
        Guid communityId,
        PaginatedQuery query,
        CancellationToken ct = default)
    {
        var queryable = _dbContext.UserCommunities
            .Where(uc => uc.CommunityId == communityId)
            .OrderBy(uc => uc.JoinedAt)
            .AsNoTracking();

        var totalItems = await queryable.CountAsync(ct);

        var items = await queryable
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(uc => new MemberDto(
                uc.UserId,
                uc.User.FullName ?? string.Empty,
                uc.User.AvatarUrl,
                uc.Role.ToString(),
                uc.JoinedAt))
            .ToListAsync(ct);

        return PaginatedResult<MemberDto>.Create(items, totalItems, query.Page, query.PageSize);
    }
    
    public void AddChannels(IEnumerable<Channel> channels) => throw new NotImplementedException();
    public Task<List<Channel>> GetChannelsAsync(Guid communityId, CancellationToken ct = default) => throw new NotImplementedException();

}