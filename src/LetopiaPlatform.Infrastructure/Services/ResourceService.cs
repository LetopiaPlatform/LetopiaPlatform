using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.CommunityResourse;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Exceptions;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Services;

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _resourceRepo;
    private readonly ITagRepository _tagRepo;
    private readonly ICommunityRepository _communityRepo;
    private readonly IGenericRepository<User> _userRepo;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly ILinkPreviewService _preview;
    private readonly ILogger<ResourceService> _logger;

    public ResourceService(
        IResourceRepository resourceRepo,
        ITagRepository tagRepo,
        ICommunityRepository communityRepo,
        IGenericRepository<User> userRepo,
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        ILinkPreviewService preview,
        ILogger<ResourceService> logger)
    {
        _resourceRepo = resourceRepo;
        _tagRepo = tagRepo;
        _communityRepo = communityRepo;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _preview = preview;
        _logger = logger;
    }

    // ── Create ────────────────────────────────────────────────────────────────

    public async Task<Result<ResourceDto>> CreateResourceAsync(
        CreateResourceRequest request,
        Guid communityId,
        Guid userId,
        CancellationToken ct = default)
    {
        await EnsureCommunityExistsAsync(communityId, ct);
        var membership = await _communityRepo.GetMembershipAsync(communityId, userId, ct)
            ?? throw new UnauthorizedException("You must be a member of this community.");

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
            throw new ValidationException("Invalid URL.");

        var existing = await _resourceRepo.GetByUrlAsync(request.Url, ct);
        if (existing is not null && existing.CommunityId == communityId)
            throw new ConflictException("Resource already exists in this community.");

        var preview = await _preview.GetPreviewAsync(request.Url);

        var resource = new CommunityResource
        {
            Id = Guid.NewGuid(),
            Title = !string.IsNullOrWhiteSpace(request.Title) ? request.Title : preview.Title ?? request.Url,
            Description = !string.IsNullOrWhiteSpace(request.Description) ? request.Description : preview.Description,
            ThumbnailUrl = preview.Image,
            Url = request.Url,
            Type = request.Type,
            CommunityId = communityId,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            ViewsCount = 0,
            LikesCount = 0,
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _resourceRepo.AddAsync(resource);
            await _unitOfWork.SaveChangesAsync(ct);

            if (request.Tags.Count > 0)
                await _tagRepo.ReplaceTagsAsync(TagTarget.Resource, resource.Id, request.Tags, ct);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        _logger.LogInformation(
            "Resource {Id} ({Type}) added to community {CommunityId} by user {UserId} (role: {Role})",
            resource.Id, resource.Type, communityId, userId, membership.Role);

        var tags = await _tagRepo.GetByTargetAsync(TagTarget.Resource, resource.Id, ct);
        var uploader = await _userRepo.GetByIdAsync(userId);
        return Result<ResourceDto>.Success(ToDto(resource, isLiked: false, uploader, tags));
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public async Task<Result<ResourceDto>> UpdateResourceAsync(
        Guid resourceId,
        UpdateResourceRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        var resource = await _resourceRepo.GetByIdWithDetailsAsync(resourceId, ct)
            ?? throw new NotFoundException("Resource not found.");

        var membership = await _communityRepo.GetMembershipAsync(resource.CommunityId, userId, ct);
        var isUploader = resource.CreatedBy == userId;
        var isPrivileged = membership?.Role is CommunityRole.Owner or CommunityRole.Moderator;

        if (!isUploader && !isPrivileged)
            throw new ForbiddenException("You are not allowed to update this resource.");

        if (!string.IsNullOrWhiteSpace(request.Url) && request.Url != resource.Url)
        {
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
                throw new ValidationException("Invalid URL.");



            var preview = await _preview.GetPreviewAsync(request.Url);

            resource.Url = request.Url;
            resource.ThumbnailUrl = preview.Image;

            // Scraped values are only used when the user didn't explicitly supply them
            if (string.IsNullOrWhiteSpace(request.Title))
                resource.Title = preview.Title ?? request.Url;

            if (string.IsNullOrWhiteSpace(request.Description))
                resource.Description = preview.Description;
        }

        // Explicit title/description always win over whatever the scrape returned above
        if (!string.IsNullOrWhiteSpace(request.Title))
            resource.Title = request.Title;

        if (!string.IsNullOrWhiteSpace(request.Description))
            resource.Description = request.Description;

        if (request.Type.HasValue)
            resource.Type = request.Type.Value;

        resource.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // null = keep existing tags; empty list = clear all tags
            if (request.Tags is not null)
                await _tagRepo.ReplaceTagsAsync(TagTarget.Resource, resourceId, request.Tags, ct);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        _logger.LogInformation(
            "Resource {Id} updated by user {UserId} (role: {Role})",
            resourceId, userId, membership?.Role.ToString() ?? "uploader");

        var tags = await _tagRepo.GetByTargetAsync(TagTarget.Resource, resourceId, ct);
        var isLiked = await _resourceRepo.IsLikedByUserAsync(resourceId, userId, ct);
        var uploader = await _userRepo.GetByIdAsync(resource.CreatedBy);
        return Result<ResourceDto>.Success(ToDto(resource, isLiked, uploader, tags));
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<Result<PaginatedResult<ResourceDto>>> GetResourcesAsync(
        Guid communityId,
        ResourceQueryParams query,
        Guid currentUserId,
        CancellationToken ct = default)
    {
        await EnsureCommunityExistsAsync(communityId, ct);
        var page = await _resourceRepo.GetResourcesByCommunityAsync(communityId, query, ct);
        var dtos = await MapToDtosAsync(page.Items, currentUserId, ct);

        return Result<PaginatedResult<ResourceDto>>.Success(
            PaginatedResult<ResourceDto>.Create(dtos, page.TotalItems, page.Page, page.PageSize));
    }

    public async Task<Result<ResourceDto>> GetResourceAsync(
        Guid resourceId,
        Guid currentUserId,
        CancellationToken ct = default)
    {
        var resource = await _resourceRepo.GetByIdWithDetailsAsync(resourceId, ct)
            ?? throw new NotFoundException("Resource not found.");

        var tags = await _tagRepo.GetByTargetAsync(TagTarget.Resource, resourceId, ct);
        var isLiked = await _resourceRepo.IsLikedByUserAsync(resourceId, currentUserId, ct);
        var uploader = await _userRepo.GetByIdAsync(resource.CreatedBy);

        return Result<ResourceDto>.Success(ToDto(resource, isLiked, uploader, tags));
    }

    public async Task<Result<PaginatedResult<ResourceDto>>> GetRecommendedAsync(
        Guid communityId,
        ResourceType type,
        ResourceQueryParams query,
        Guid currentUserId,
        CancellationToken ct = default)
    {
        await EnsureCommunityExistsAsync(communityId, ct);
        var page = await _resourceRepo.GetRecommendedAsync(communityId, type, query.Page, query.PageSize, ct);
        var dtos = await MapToDtosAsync(page.Items, currentUserId, ct);

        return Result<PaginatedResult<ResourceDto>>.Success(
            PaginatedResult<ResourceDto>.Create(dtos, page.TotalItems, page.Page, page.PageSize));
    }

    // ── Engagement ────────────────────────────────────────────────────────────

    public async Task<Result> AddViewAsync(
        Guid resourceId,
        CancellationToken ct = default)
    {
        if (await _resourceRepo.GetByIdAsync(resourceId) is null)
            throw new NotFoundException("Resource not found.");

        // ExecuteUpdateAsync is self-saving — no SaveChanges needed
        await _resourceRepo.IncrementViewsAsync(resourceId, ct);
        return Result.Success();
    }

    public async Task<Result> ToggleLikeAsync(
        Guid resourceId,
        Guid userId,
        CancellationToken ct = default)
    {
        var resource = await _resourceRepo.GetByIdAsync(resourceId)
            ?? throw new NotFoundException("Resource not found.");

        if (await _communityRepo.GetMembershipAsync(resource.CommunityId, userId, ct) is null)
            throw new UnauthorizedException("You must be a member to like resources.");

        var liked = await _resourceRepo.IsLikedByUserAsync(resourceId, userId, ct);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (liked)
                await _resourceRepo.RemoveLikeAsync(resourceId, userId, ct);
            else
                await _resourceRepo.AddLikeAsync(resourceId, userId, ct);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        return Result.Success();
    }

    public async Task<Result> DeleteResourceAsync(
        Guid resourceId,
        Guid userId,
        CancellationToken ct = default)
    {
        var resource = await _resourceRepo.GetByIdAsync(resourceId)
            ?? throw new NotFoundException("Resource not found.");

        var membership = await _communityRepo.GetMembershipAsync(resource.CommunityId, userId, ct);
        var isUploader = resource.CreatedBy == userId;
        var isPrivileged = membership?.Role is CommunityRole.Owner or CommunityRole.Moderator;

        if (!isUploader && !isPrivileged)
            throw new ForbiddenException("You are not allowed to delete this resource.");

        resource.IsDeleted = true;
        resource.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Resource {Id} soft-deleted by user {UserId} (role: {Role})",
            resourceId, userId, membership?.Role.ToString() ?? "uploader");

        return Result.Success();
    }

    // ── Private helpers ───────────────────────────────────────────────────────
    private async Task EnsureCommunityExistsAsync(Guid communityId, CancellationToken ct)
    {
        var exists = await _communityRepo.GetByIdAsync(communityId, ct);
        if (exists is null)
            throw new NotFoundException($"Community {communityId} not found.");
    }
    private async Task<List<ResourceDto>> MapToDtosAsync(
        IEnumerable<CommunityResource> resources,
        Guid currentUserId,
        CancellationToken ct)
    {
        var list = resources.ToList();
        if (list.Count == 0) return [];

        var resourceIds = list.Select(r => r.Id).ToList();

        // Single query for all tags on this page
        var tagLookup = await _tagRepo.GetByTargetsAsync(TagTarget.Resource, resourceIds, ct);

        // Single query for all liked resource IDs — O(1) lookup per resource
        var likedIds = await _resourceRepo.GetLikedResourceIdsAsync(currentUserId, resourceIds, ct);

        // Cache uploaders within this page to avoid duplicate DB hits
        var uploaderCache = new Dictionary<Guid, User?>();

        var dtos = new List<ResourceDto>(list.Count);
        foreach (var r in list)
        {
            if (!uploaderCache.TryGetValue(r.CreatedBy, out var uploader))
            {
                uploader = await _userRepo.GetByIdAsync(r.CreatedBy);
                uploaderCache[r.CreatedBy] = uploader;
            }

            dtos.Add(ToDto(r, likedIds.Contains(r.Id), uploader, tagLookup[r.Id]));
        }

        return dtos;
    }

    private static ResourceDto ToDto(
        CommunityResource r,
        bool isLiked,
        User? uploader,
        IEnumerable<Tag> tags) => new()
        {
            Id = r.Id,
            Title = r.Title,
            Url = r.Url,
            ThumbnailUrl = r.ThumbnailUrl,
            Description = r.Description,
            Type = r.Type,
            ViewsCount = r.ViewsCount,
            LikesCount = r.LikesCount,
            IsLikedByCurrentUser = isLiked,
            Tags = tags.Select(t => t.TagName).ToList(),
            UploadedBy = new UploadedByDto(
                r.CreatedBy,
                uploader?.UserName ?? "Unknown",
                uploader?.AvatarUrl),
            CreatedAt = r.CreatedAt,
        };
}
