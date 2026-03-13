using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.CommunityResourse;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;
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
        var membership = await _communityRepo.GetMembershipAsync(communityId, userId, ct);
        if (membership is null)
            return Result<ResourceDto>.Failure("You must be a member of this community.");

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
            return Result<ResourceDto>.Failure("Invalid URL.");

        var existing = await _resourceRepo.GetByUrlAsync(request.Url, ct);
        if (existing is not null && existing.CommunityId == communityId)
            return Result<ResourceDto>.Failure("Resource already exists in this community.");

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
            Tags = request.Tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => new ResourceTag { Id = Guid.NewGuid(), TagName = t.Trim().ToLowerInvariant() })
                .ToList(),
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _resourceRepo.AddAsync(resource);
            await _unitOfWork.SaveChangesAsync(ct);     // persist resource first to get its Id

            if (request.Tags.Count > 0)
                await _tagRepo.ReplaceTagsAsync(TagTarget.Resource, resource.Id, request.Tags, ct);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error creating resource for community {CommunityId}", communityId);
            return Result<ResourceDto>.Failure("Error creating resource.");
        }

        _logger.LogInformation(
            "Resource {Id} ({Type}) added to community {CommunityId} by user {UserId} (role: {Role})",
            resource.Id, resource.Type, communityId, userId, membership.Role);

        var tags = await _tagRepo.GetByTargetAsync(TagTarget.Resource, resource.Id, ct);
        var uploader = await _userRepo.GetByIdAsync(userId);
        return Result<ResourceDto>.Success(ToDto(resource, false, uploader, tags));
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public async Task<Result<ResourceDto>> UpdateResourceAsync(
        Guid resourceId,
        UpdateResourceRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        var resource = await _resourceRepo.GetByIdWithDetailsAsync(resourceId, ct);
        if (resource is null)
            return Result<ResourceDto>.Failure("Resource not found.");

        // Only the uploader, Owner, or Moderator can update
        var membership = await _communityRepo.GetMembershipAsync(resource.CommunityId, userId, ct);
        var isUploader = resource.CreatedBy == userId;
        var isPrivileged = membership?.Role is CommunityRole.Owner or CommunityRole.Moderator;

        if (!isUploader && !isPrivileged)
            return Result<ResourceDto>.Failure("You are not allowed to update this resource.");

        // URL change: re-validate, re-check duplicates, re-scrape preview
        if (!string.IsNullOrWhiteSpace(request.Url) && request.Url != resource.Url)
        {
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
                return Result<ResourceDto>.Failure("Invalid URL.");

            var duplicate = await _resourceRepo.GetByUrlAsync(request.Url, ct);
            if (duplicate is not null && duplicate.CommunityId == resource.CommunityId && duplicate.Id != resourceId)
                return Result<ResourceDto>.Failure("This URL already exists in this community.");

            var preview = await _preview.GetPreviewAsync(request.Url);

            resource.Url = request.Url;

            // Re-apply preview fields only when the user hasn't explicitly overridden them
            // in this same request — their explicit values take priority
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

        // Replace tags when a new list is provided
        if (request.Tags is not null)
        {
            resource.Tags.Clear();
            foreach (var tag in request.Tags.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                resource.Tags.Add(new ResourceTag
                {
                    Id = Guid.NewGuid(),
                    ResourceId = resource.Id,
                    TagName = tag.Trim().ToLowerInvariant(),
                });
            }
        }

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
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error updating resource {ResourceId}", resourceId);
            return Result<ResourceDto>.Failure("Error updating resource.");
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
        var resource = await _resourceRepo.GetByIdWithDetailsAsync(resourceId, ct);
        if (resource is null)
            return Result<ResourceDto>.Failure("Resource not found.");

        var tags = await _tagRepo.GetByTargetAsync(TagTarget.Resource, resourceId, ct);
        var isLiked = await _resourceRepo.IsLikedByUserAsync(resourceId, currentUserId, ct);
        var uploader = await _userRepo.GetByIdAsync(resource.CreatedBy);

        return Result<ResourceDto>.Success(ToDto(resource, isLiked, uploader, tags));
    }

    public async Task<Result<PaginatedResult<ResourceDto>>> GetRecommendedAsync(
        Guid communityId,
       
        ResourceQueryParams query,
        Guid currentUserId,
        CancellationToken ct = default)
    {
        var page = await _resourceRepo.GetRecommendedAsync(communityId, query.Type??ResourceType.Video, query.Page, query.PageSize, ct);
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
            return Result.Failure("Resource not found.");

        // ExecuteUpdateAsync is self-saving — no SaveChanges needed
        await _resourceRepo.IncrementViewsAsync(resourceId, ct);
        return Result.Success();
    }

    public async Task<Result> ToggleLikeAsync(
        Guid resourceId,
        Guid userId,
        CancellationToken ct = default)
    {
        var resource = await _resourceRepo.GetByIdAsync(resourceId);
        if (resource is null)
            return Result.Failure("Resource not found.");

        var membership = await _communityRepo.GetMembershipAsync(resource.CommunityId, userId, ct);
        if (membership is null)
            return Result.Failure("You must be a member to like resources.");

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
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Toggle like failed for resource {ResourceId} user {UserId}", resourceId, userId);
            return Result.Failure("Like update failed.");
        }

        return Result.Success();
    }

    public async Task<Result> DeleteResourceAsync(
        Guid resourceId,
        Guid userId,
        CancellationToken ct = default)
    {
        var resource = await _resourceRepo.GetByIdAsync(resourceId);
        if (resource is null)
            return Result.Failure("Resource not found.");

        var membership = await _communityRepo.GetMembershipAsync(resource.CommunityId, userId, ct);
        var isUploader = resource.CreatedBy == userId;
        var isPrivileged = membership?.Role is CommunityRole.Owner or CommunityRole.Moderator;

        if (!isUploader && !isPrivileged)
            return Result.Failure("You are not allowed to delete this resource.");

        resource.IsDeleted = true;
        resource.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Resource {Id} soft-deleted by user {UserId} (role: {Role})",
            resourceId, userId, membership?.Role.ToString() ?? "uploader");

        return Result.Success();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Maps a page of resources to DTOs.
    /// Tags are batch-loaded in one query via <see cref="ITagRepository.GetByTargetsAsync"/>.
    /// Uploaders are cached per page to avoid repeated DB hits for the same user.
    /// </summary>
    private async Task<List<ResourceDto>> MapToDtosAsync(
        IEnumerable<CommunityResource> resources,
        Guid currentUserId,
        CancellationToken ct)
    {
        var list = resources.ToList();
        if (list.Count == 0) return [];

        // Single query for all tags on this page — ILookup<Guid, Tag> for O(1) per-resource access
        var resourceIds = list.Select(r => r.Id).ToList();
        var tagLookup = await _tagRepo.GetByTargetsAsync(TagTarget.Resource, resourceIds, ct);

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

            var tags = tagLookup[r.Id].ToList();
            var isLiked = await _resourceRepo.IsLikedByUserAsync(r.Id, currentUserId, ct);
            dtos.Add(ToDto(r, isLiked, uploader, tags));
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
