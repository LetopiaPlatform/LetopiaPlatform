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
                CategoryId = request.CategoryId,
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

            // Load the Category navigation for the response
            community = await _communityRepository.GetByIdAsync(community.Id, ct) ?? community;

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

    public async Task<CommunityDetailDto> UpdateAsync(
        Guid communityId,
        UpdateCommunityRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        var community = await _communityRepository.GetByIdAsync(communityId, ct)
            ?? throw new NotFoundException("Community", communityId.ToString());

        var membership = await _communityRepository.GetMembershipAsync(communityId, userId, ct)
            ?? throw new ForbiddenException("You are not a member of this community.");
        
        if (membership.Role is not (CommunityRole.Owner or CommunityRole.Moderator))
            throw new ForbiddenException("Only the owner or a moderator can update community settings.");
        
        if (request.Name is not null) community.Name = request.Name;
        if (request.Description is not null) community.Description = request.Description;
        if (request.IconUrl is not null) community.IconUrl = request.IconUrl;
        if (request.CoverImageUrl is not null) community.CoverImageUrl = request.CoverImageUrl;
        if (request.IsPrivate.HasValue) community.IsPrivate = request.IsPrivate.Value;

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Community Service - Community '{CommunityName}' (ID: {CommunityId}) updated by user {UserId}",
            community.Name, community.Id, userId);
        
        var channels = await _communityRepository.GetChannelsAsync(community.Id, ct);
        return MapToDetail(community, isMember: true, userRole: membership.Role.ToString(), channels: BuildChannelTree(channels));
    }

    public async Task JoinAsync(
        Guid communityId,
        Guid userId,
        CancellationToken ct = default)
    {
        var community = await _communityRepository.GetByIdAsync(communityId, ct)
            ?? throw new NotFoundException("Community", communityId);

        if (community.IsPrivate)
            throw new ForbiddenException("This community is private. You need an invitation to join.");

        if (await _communityRepository.IsMemberAsync(communityId, userId, ct))
            throw new ConflictException("You are already a member of this community.");

        var membership = new UserCommunity
        {
            UserId = userId,
            CommunityId = communityId,
            Role = CommunityRole.Member,
            JoinedAt = DateTime.UtcNow
        };

        _communityRepository.AddMember(membership);
        await _unitOfWork.SaveChangesAsync(ct);
        await _communityRepository.IncrementMemberCountAsync(communityId, ct);

        _logger.LogInformation(
            "Community Service - User {UserId} joined community {CommunityId}", userId, communityId);
    }

    public async Task LeaveAsync(
        Guid communityId,
        Guid userId,
        CancellationToken ct = default)
    {
        var membership = await _communityRepository.GetMembershipAsync(communityId, userId, ct)
            ?? throw new NotFoundException("You are not a member of this community.");
        
        if (membership.Role == CommunityRole.Owner)
            throw new AppException("The owner cannot leave the community. Transfer ownership first.", 400);
        
        _communityRepository.RemoveMember(membership);
        await _unitOfWork.SaveChangesAsync(ct);
        await _communityRepository.DecrementMemberCountAsync(communityId, ct);

        _logger.LogInformation(
            "Community Service - User {UserId} left community {CommunityId}", userId, communityId);
    }

    public async Task<PaginatedResult<MemberDto>> GetMembersAsync(
        Guid communityId,
        PaginatedQuery query,
        CancellationToken ct = default)
    {
        _ = await _communityRepository.GetByIdAsync(communityId, ct)
            ?? throw new NotFoundException("Community", communityId);

        return await _communityRepository.GetMembersAsync(communityId, query, ct);
    }

    public async Task ChangeRoleAsync(
        Guid communityId,
        Guid targetUserId,
        ChangeRoleRequest request,
        Guid callerUserId, CancellationToken ct = default)
    {
        if (!Enum.TryParse<CommunityRole>(request.Role, out var newRole))
            throw new AppException($"Invalid role: {request.Role}", 400);

        var callerMembership = await _communityRepository.GetMembershipAsync(communityId, callerUserId, ct)
            ?? throw new ForbiddenException("You are not a member of this community.");

        if (callerMembership.Role != CommunityRole.Owner)
            throw new ForbiddenException("Only the owner can change member roles.");

        var targetMembership = await _communityRepository.GetMembershipAsync(communityId, targetUserId, ct)
            ?? throw new NotFoundException("Member not found in this community.");

        if (callerUserId == targetUserId && newRole != CommunityRole.Owner)
            throw new AppException("You cannot demote yourself. Transfer ownership first.", 400);

        if (newRole == CommunityRole.Owner)
        {
            callerMembership.Role = CommunityRole.Moderator;
            targetMembership.Role = CommunityRole.Owner;

            _logger.LogInformation(
                "Community Service - Ownership of community {CommunityId} transferred from {OldOwner} to {NewOwner}",
                communityId, callerUserId, targetUserId);
        }
        else
        {
            targetMembership.Role = newRole;

            _logger.LogInformation(
                "Community Service -Role of user {TargetUserId} in community {CommunityId} changed to {NewRole} by {CallerId}",
                targetUserId, communityId, newRole, callerUserId);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

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
            c.Id, c.Name, c.Slug, c.Description,
            c.CategoryId, c.Category?.Name ?? string.Empty,
            c.IconUrl, c.CoverImageUrl,
            c.MemberCount, c.PostCount, c.IsPrivate,
            c.CreatedAt, c.LastPostAt,
            isMember, userRole, channels);
    }
}