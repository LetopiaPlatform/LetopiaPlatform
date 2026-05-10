using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Author;
using LetopiaPlatform.Core.DTOs.Post;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Exceptions;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Services.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
namespace LetopiaPlatform.Infrastructure.Services;
public class PostService : IPostService
{
    private readonly IPostRepository _postRepo;
    private readonly ICommunityRepository _communityRepo;
    private readonly IReactionRepository _reactionRepo;
    private readonly ITagRepository _tagRepo;
    private readonly IPostAuthorizationService _authorization;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly ILogger<PostService> _logger;

    public PostService(
        IPostRepository postRepo,
        ICommunityRepository communityRepo,
        IReactionRepository reactionRepo,
        ITagRepository tagRepo,
        IPostAuthorizationService authorization,
        IFileStorageService fileStorage,
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        ILogger<PostService> logger)
    {
        _postRepo = postRepo;
        _communityRepo = communityRepo;
        _reactionRepo = reactionRepo;
        _tagRepo = tagRepo;
        _authorization = authorization;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // ─────────────────────────────
    // CREATE
    // ─────────────────────────────



    public async Task<Result<PostDetailDto>> CreateAsync(
        Guid communityId,
        Guid channelId,
        CreatePostRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        await EnsureCommunityExistsAsync(communityId, ct);

        var membership = await _communityRepo.GetMembershipAsync(communityId, userId, ct)
            ?? throw new UnauthorizedException("You must be a member of this community.");

        var channels = await _communityRepo.GetChannelsAsync(communityId, ct);
        var channel = channels.FirstOrDefault(c => c.Id == channelId)
            ?? throw new NotFoundException($"Channel {channelId} not found.");

        if (!_authorization.CanCreate(request.PostType, channel.ChannelType, membership))
            throw new ForbiddenException("You are not allowed to create this type of post.");

        var uploadedUrls = await UploadImagesAsync(request.Images, ct);

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            PostType = request.PostType,
            CommunityId = communityId,
            ChannelId = channelId,
            AuthorId = membership.Id,
            CreatedAt = DateTime.UtcNow,
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _postRepo.AddAsync(post);

            // Assign after tracking starts so EF detects the value
            post.ImageUrls = uploadedUrls;

            await _unitOfWork.SaveChangesAsync(ct);

            if (request.Tags.Count > 0)
                await _tagRepo.ReplaceTagsAsync(TagTarget.Post, post.Id, request.Tags, ct);
            await _communityRepo.IncrementPostCountAsync(communityId, 1, ct);
            await _unitOfWork.SaveChangesAsync(ct);
           
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            await DeleteUploadedFilesAsync(uploadedUrls, ct);
            throw;
        }

        return await GetByIdAsync(post.Id, userId, ct);
    }

    // ─────────────────────────────
    // UPDATE
    // ─────────────────────────────

    public async Task<Result<PostDetailDto>> UpdateAsync(
        Guid postId,
        UpdatePostRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        var post = await GetPostOrThrowAsync(postId);

        var membership = await _communityRepo.GetMembershipAsync(post.CommunityId, userId, ct);
        if (membership is null || post.AuthorId != membership.Id)
            throw new ForbiddenException("You are not allowed to edit this post.");

        // Upload before opening transaction so we can rollback cleanly on failure
        var newlyUploadedFiles = await UploadImagesAsync(request.AddImages, ct);
        var filesToDelete = new List<string>();

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Build a new list so EF detects the reference change
            post.ImageUrls = post.ImageUrls
                .Where(url =>
                {
                    if (!request.RemoveImageUrls.Contains(url)) return true;
                    filesToDelete.Add(url);
                    return false;
                })
                .Concat(newlyUploadedFiles)
                .ToList();

            post.Title = request.Title ?? post.Title;
            post.Content = request.Content ?? post.Content;
            post.UpdatedAt = DateTime.UtcNow;

            if (request.Tags is not null)
                await _tagRepo.ReplaceTagsAsync(TagTarget.Post, postId, request.Tags, ct);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            // Rollback only newly uploaded files — originals are untouched in storage
            await DeleteUploadedFilesAsync(newlyUploadedFiles, ct);
            throw;
        }

        // Post-commit: physically delete removed images from storage
        await DeleteUploadedFilesAsync(filesToDelete, ct);

        return await GetByIdAsync(postId, userId, ct);
    }

    // ─────────────────────────────
    // LIST
    // ─────────────────────────────

    public async Task<Result<PaginatedResult<PostSummaryDto>>> ListAsync(
        Guid communityId,
        Guid channelId,
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        CancellationToken ct = default)
    {
        await EnsureCommunityExistsAsync(communityId, ct);
        var result = await _postRepo.GetPagedAsync(
            new PaginatedQuery { Page = page, PageSize = pageSize },
            communityId, channelId, search, sortBy, ct);

        var posts = result.Items.ToList();
        var tagLookup = await _tagRepo.GetByTargetsAsync(
            TagTarget.Post, posts.Select(p => p.Id).ToList(), ct);

        var dtos = posts
            .Select(p => MapToPostSummaryDto(p, null, tagLookup[p.Id]))
            .ToList();

        return Result<PaginatedResult<PostSummaryDto>>.Success(
            PaginatedResult<PostSummaryDto>.Create(dtos, result.TotalItems, page, pageSize));
    }

    // ─────────────────────────────
    // GET BY ID
    // ─────────────────────────────

    public async Task<Result<PostDetailDto>> GetByIdAsync(
        Guid postId,
        Guid? currentUserId = null,
        CancellationToken ct = default)
    {
        var post = await GetPostOrThrowAsync(postId, includeAuthor: true);
        var tags = await _tagRepo.GetByTargetAsync(TagTarget.Post, postId, ct);

        string? reaction = null;
        if (currentUserId.HasValue)
        {
            var userReaction = await _reactionRepo.GetUserReactionAsync(
                currentUserId.Value, TargetType.Post, postId);
            reaction = userReaction?.ReactionType.ToString();
        }

        return Result<PostDetailDto>.Success(MapToPostDetailDto(post, reaction, tags));
    }

    // ─────────────────────────────
    // PINNED
    // ─────────────────────────────

    public async Task<Result<IReadOnlyList<PostSummaryDto>>> GetPinnedAsync(
        Guid communityId,
        Guid channelId,
        CancellationToken ct = default)
    {
        await EnsureCommunityExistsAsync(communityId, ct);
        var posts = await _postRepo.GetPinnedAsync(communityId, channelId, ct);
        if (!posts.Any())
            return Result<IReadOnlyList<PostSummaryDto>>.Success([]);

        var tagLookup = await _tagRepo.GetByTargetsAsync(
            TagTarget.Post, posts.Select(p => p.Id).ToList(), ct);

        var dtos = posts
            .Select(p => MapToPostSummaryDto(p, null, tagLookup[p.Id]))
            .ToList();

        return Result<IReadOnlyList<PostSummaryDto>>.Success(dtos);
    }
    // ─────────────────────────────
    // TOGGLE PIN
    // ─────────────────────────────

    public async Task<Result> TogglePinAsync(
        Guid postId,
        Guid userId,
        CancellationToken ct = default)
    {
        var post = await GetPostOrThrowAsync(postId);

        var membership = await _communityRepo.GetMembershipAsync(post.CommunityId, userId, ct)
            ?? throw new UnauthorizedException("You must be a member of this community.");

        if (!_authorization.CanPin(membership))
            throw new ForbiddenException("You are not allowed to pin or unpin posts.");

        post.IsPinned = !post.IsPinned;
        post.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }





    // ─────────────────────────────
    // DELETE
    // ─────────────────────────────

    public async Task<Result> DeleteAsync(
        Guid postId,
        Guid userId,
        CancellationToken ct = default)
    {
        var post = await GetPostOrThrowAsync(postId);

        var membership = await _communityRepo.GetMembershipAsync(post.CommunityId, userId, ct);
        if (membership is null || post.AuthorId != membership.Id)
            throw new ForbiddenException("You are not allowed to delete this post.");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            await _communityRepo.IncrementPostCountAsync(post.CommunityId, -1, ct);

            post.IsDeleted = true;
            post.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);

            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    // ─────────────────────────────
    // PRIVATE HELPERS
    // ─────────────────────────────
    private async Task EnsureCommunityExistsAsync(Guid communityId, CancellationToken ct)
    {
        var exists = await _communityRepo.GetByIdAsync(communityId, ct);
        if (exists is null)
            throw new NotFoundException($"Community {communityId} not found.");
    }
    private async Task<Post> GetPostOrThrowAsync(Guid postId, bool includeAuthor = false)
    {
        var post = includeAuthor
            ? await _postRepo.GetByIdAsync(postId, "Author.User")
            : await _postRepo.GetByIdAsync(postId);

        if (post is null || post.IsDeleted)
            throw new NotFoundException($"Post {postId} not found.");

        return post;
    }

    private async Task<List<string>> UploadImagesAsync(
        IEnumerable<IFormFile> images,
        CancellationToken ct)
    {
        var urls = new List<string>();
        foreach (var image in images)
        {
            var upload = await _fileStorage.UploadAsync(image, "posts", ct);
            if (!upload.IsSuccess || upload.Value is null)
                throw new ValidationException($"Image upload failed: {image.FileName}");

            urls.Add(upload.Value);
        }
        return urls;
    }

    private async Task DeleteUploadedFilesAsync(IEnumerable<string> urls, CancellationToken ct)
    {
        foreach (var url in urls)
        {
            try { await _fileStorage.DeleteAsync(url, ct); }
            catch (Exception ex) when (ex is not OperationCanceledException && ex is not TaskCanceledException)
            {
                var safeUrl = url
                    .Replace("\r", "", StringComparison.Ordinal)
                    .Replace("\n", "", StringComparison.Ordinal);

                if (safeUrl.Length > 200)
                    safeUrl = safeUrl[..200];

                _logger.LogWarning(ex, "Failed to delete orphaned file {Url}", safeUrl);
            }
        }
    }
    // ─────────────────────────────
    // MAPPING
    // ─────────────────────────────

    private static AuthorDto MapAuthor(Post post)
    {
        var user = post.Author?.User;
        return new AuthorDto(
            user?.Id ?? Guid.Empty,
            user?.FullName ?? "Unknown",
            user?.AvatarUrl,
            post.Author?.Role);
    }

    private static PostDetailDto MapToPostDetailDto(Post post, string? reaction, IEnumerable<Tag> tags)
        => new(
            post.Id,
            post.Title ?? "",
            post.Content ?? "",
            post.ImageUrls,
            MapAuthor(post),
            post.PostType,
            post.Upvotes,
            post.CommentCount,
            post.ViewsCount,
            post.IsPinned,
            post.CreatedAt,
            reaction,
            post.UpdatedAt,
            tags.Select(t => t.TagName).ToList());

    private static PostSummaryDto MapToPostSummaryDto(Post post, string? reaction, IEnumerable<Tag> tags)
    {
        var summary = post.Content?.Length > 200
            ? post.Content[..200] + "..."
            : post.Content ?? "";

        return new PostSummaryDto(
            post.Id,
            post.Title ?? "",
            summary,
            post.ImageUrls,
            MapAuthor(post),
            post.PostType,
            post.Upvotes,
            post.CommentCount,
            post.ViewsCount,
            post.IsPinned,
            post.CreatedAt,
            reaction,
            tags.Select(t => t.TagName).ToList());
    }
}
