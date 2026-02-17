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

    public Task<PaginatedResult<MemberDto>> GetMembersAsync(Guid communityId, PaginatedQuery query, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<UserCommunity?> GetMembershipAsync(Guid communityId, Guid userId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<PaginatedResult<CommunitySummaryDto>> ListAsync(PaginatedQuery query, string? category = null, string? search = null, string? sortBy = null, CancellationToken ct = default) => throw new NotImplementedException();

    public void AddMember(UserCommunity membership) => throw new NotImplementedException();
    public Task DecrementMemberCountAsync(Guid communityId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task IncrementMemberCountAsync(Guid communityId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<bool> IsMemberAsync(Guid communityId, Guid userId, CancellationToken ct = default) => throw new NotImplementedException();
    public void RemoveMember(UserCommunity membership) => throw new NotImplementedException();
    public void AddChannels(IEnumerable<Channel> channels) => throw new NotImplementedException();
    public Task<List<Channel>> GetChannelsAsync(Guid communityId, CancellationToken ct = default) => throw new NotImplementedException();

}