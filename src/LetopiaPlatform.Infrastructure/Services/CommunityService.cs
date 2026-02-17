using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Community;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Services;

public class CommunityService : ICommunityService
{
    private readonly ICommunityRepository _communityRepository;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly ILogger<CommunityService> _logger;
    
    public CommunityService(
        ICommunityRepository communityRepository,
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        ILogger<CommunityService> logger)
    {
        _communityRepository = communityRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public Task ChangeRoleAsync(Guid communityId, Guid targetUserId, ChangeRoleRequest request, Guid callerUserId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<CommunityDetailDto> CreateAsync(CreateCommunityRequest request, Guid userId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<CommunityDetailDto> GetBySlugAsync(string slug, Guid? currentUserId = null, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<PaginatedResult<MemberDto>> GetMembersAsync(Guid communityId, PaginatedQuery query, CancellationToken ct = default) => throw new NotImplementedException();
    public Task JoinAsync(Guid communityId, Guid userId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task LeaveAsync(Guid communityId, Guid userId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<PaginatedResult<CommunitySummaryDto>> ListAsync(PaginatedQuery query, string? category = null, string? search = null, string? sortBy = null, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<CommunityDetailDto> UpdateAsync(Guid communityId, UpdateCommunityRequest request, Guid userId, CancellationToken ct = default) => throw new NotImplementedException();
}