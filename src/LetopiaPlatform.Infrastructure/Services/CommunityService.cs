using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Community;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Exceptions;
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

    public async Task<CommunityDetailDto> CreateAsync(
        CreateCommunityRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        var slug = await SlugGenerator.GenerateUniqueAsync(
            request.Name,
            async candidate => await _communityRepository.SlugExistsAsync(candidate, ct));
        
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var community = new Community
            {
                Name = request.Name,
                Slug = slug,
                Description = request.Description,
                TopicCategory = request.TopicCategory,
                IconUrl = request.IconUrl,
                IsPrivate = request.IsPrivate,
                CreatedBy = userId,
                MemberCount = 1
            };

            _communityRepository.AddCommunity(community);
            await _unitOfWork.SaveChangesAsync(ct);

            var membership = new UserCommunity
            {
                CommunityId = community.Id,
                UserId = userId,
                Role = CommunityRole.Owner,
                JoinedAt = DateTime.UtcNow
            };

            _communityRepository.AddMember(membership);

            var defaultChannels = CreateDefaultChannels(community.Id);
            _communityRepository.AddChannels(defaultChannels);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation(
                "Community Service - Community '{CommunityName}' (slug: {Slug}) created by user {UserId}",
                community.Name, community.Slug, userId);
            
            var channelDto = defaultChannels
                .Select(ch => MapToChannelSummary(ch))
                .ToList();

            return MapToDetail(community, isMember: true, userRole: membership.Role.ToString(), channels: channelDto);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<PaginatedResult<CommunitySummaryDto>> ListAsync(
        PaginatedQuery query,
        string? category = null,
        string? search = null,
        string? sortBy = null,
        CancellationToken ct = default)
    {
        return await _communityRepository.ListAsync(query, category, search, sortBy, ct);
    }
    public async Task<CommunityDetailDto> GetBySlugAsync(
        string slug,
        Guid? currentUserId = null,
        CancellationToken ct = default)
    {
        var community = await _communityRepository.GetBySlugAsync(slug, ct)
            ?? throw new NotFoundException("Community", slug);
        
        var channels = await _communityRepository.GetChannelsAsync(community.Id, ct);

        bool isMember = false;
        string? userRole = null;

        if (currentUserId.HasValue)
        {
            var membership = await _communityRepository.GetMembershipAsync(community.Id, currentUserId.Value, ct);
            if (membership is not null)
            {
                isMember = true;
                userRole = membership.Role.ToString();
            }
        }

        var channelDtos = BuildChannelTree(channels);
        return MapToDetail(community, isMember, userRole, channelDtos);
    }

    public Task ChangeRoleAsync(Guid communityId, Guid targetUserId, ChangeRoleRequest request, Guid callerUserId, CancellationToken ct = default) => throw new NotImplementedException();

    public Task<PaginatedResult<MemberDto>> GetMembersAsync(Guid communityId, PaginatedQuery query, CancellationToken ct = default) => throw new NotImplementedException();
    public Task JoinAsync(Guid communityId, Guid userId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task LeaveAsync(Guid communityId, Guid userId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<CommunityDetailDto> UpdateAsync(Guid communityId, UpdateCommunityRequest request, Guid userId, CancellationToken ct = default) => throw new NotImplementedException();

    // Private helpers
    private static List<Channel> CreateDefaultChannels(Guid communityId)
    {
        return
        [
            new Channel
            {
                CommunityId = communityId,
                Name = "Announcements",
                Slug = "announcements",
                ChannelType = ChannelType.Announcement,
                AllowMemberPosts = false,
                AllowComments = true,
                IsDefault = true,
                DisplayOrder = 1
            },
            new Channel
            {
                CommunityId = communityId,
                Name = "General",
                Slug = "general",
                ChannelType = ChannelType.Discussion,
                AllowMemberPosts = true,
                AllowComments = true,
                IsDefault = true,
                DisplayOrder = 2
            }
        ];
    }

    private static List<ChannelSummaryDto> BuildChannelTree(List<Channel> channels)
    {
        var lookup = channels.ToLookup(ch => ch.ParentId);

        return lookup[null]
            .Select(parent => MapToChannelSummary(parent, lookup))
            .ToList();
    }
    private static ChannelSummaryDto MapToChannelSummary(
        Channel ch,
        ILookup<Guid?, Channel>? lookup = null)
    {
        var subChannels = lookup is not null
            ? lookup[ch.Id].Select(sub => MapToChannelSummary(sub)).ToList()
            : [];

        return new ChannelSummaryDto(
            ch.Id,
            ch.Name,
            ch.Slug,
            ch.ChannelType.ToString(),
            ch.Description,
            ch.PostCount,
            ch.IsDefault,
            ch.IsArchived,
            ch.AllowMemberPosts,
            ch.AllowComments,
            ch.DisplayOrder,
            subChannels);
    }

    private static CommunityDetailDto MapToDetail(
        Community c, bool isMember, string? userRole, List<ChannelSummaryDto> channels)
    {
        return new CommunityDetailDto(
            c.Id, c.Name, c.Slug, c.Description, c.TopicCategory,
            c.IconUrl, c.CoverImageUrl,
            c.MemberCount, c.PostCount, c.IsPrivate,
            c.CreatedAt, c.LastPostAt,
            isMember, userRole, channels);
    }
}