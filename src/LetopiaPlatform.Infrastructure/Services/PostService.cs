using System.Threading.Channels;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Author;
using LetopiaPlatform.Core.DTOs.Post;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Core.Services.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
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
    #region create
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
        var membership = await _communityRepo.GetMembershipAsync(communityId, userId, ct);

        if (membership is null)
            return Result<PostDetailDto>.Failure("You must be a member of this community.");

        var channels = await _communityRepo.GetChannelsAsync(communityId, ct);
        var channel = channels.FirstOrDefault(c => c.Id == channelId);

        if (channel is null)
            return Result<PostDetailDto>.Failure("Channel not found.");

        if (!_authorization.CanCreate(request.PostType, channel.ChannelType, membership))
            return Result<PostDetailDto>.Failure("You are not allowed to create this post.");

        var uploadedUrls = new List<string>();

        foreach (var image in request.Images)
        {
            var upload = await _fileStorage.UploadAsync(image, "posts", ct);

            if (!upload.IsSuccess || upload.Value is null)
                return Result<PostDetailDto>.Failure($"Image upload failed: {image.FileName}");

            uploadedUrls.Add(upload.Value);
        }

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
            ImageUrls = uploadedUrls
        };

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            await _postRepo.AddAsync(post);
            await _unitOfWork.SaveChangesAsync(ct);

            if (request.Tags.Count > 0)
                await _tagRepo.ReplaceTagsAsync(TagTarget.Post, post.Id, request.Tags, ct);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();

            foreach (var url in uploadedUrls)
                await _fileStorage.DeleteAsync(url, ct);

            _logger.LogError(ex, "Post creation failed");

            return Result<PostDetailDto>.Failure("Error creating post.");
        }

        return await GetByIdAsync(post.Id, userId, ct);
    }
    #endregion
    #region quers
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
        var result = await _postRepo.GetPagedAsync(
            new PaginatedQuery { Page = page, PageSize = pageSize },
            communityId,
            channelId,
            search,
            sortBy,
            ct);

        var posts = result.Items.ToList();
        var postIds = posts.Select(p => p.Id).ToList();

        var tagLookup = await _tagRepo.GetByTargetsAsync(TagTarget.Post, postIds, ct);

        var dtos = posts.Select(p =>
            MapToPostSummaryDto(
                p,
                null,
                tagLookup[p.Id]))
            .ToList();

        return Result<PaginatedResult<PostSummaryDto>>.Success(
            PaginatedResult<PostSummaryDto>.Create(
                dtos,
                result.TotalItems,
                page,
                pageSize));
    }

    // ─────────────────────────────
    // GET BY ID
    // ─────────────────────────────

    public async Task<Result<PostDetailDto>> GetByIdAsync(
        Guid postId,
        Guid? currentUserId = null,
        CancellationToken ct = default)
    {
        var post = await _postRepo.GetByIdAsync(postId, "Author.User");

        if (post is null || post.IsDeleted)
            return Result<PostDetailDto>.Failure("Post not found.");

        var tags = await _tagRepo.GetByTargetAsync(TagTarget.Post, postId, ct);

        string? reaction = null;

        if (currentUserId.HasValue)
        {
            var userReaction = await _reactionRepo.GetUserReactionAsync(
                currentUserId.Value,
                TargetType.Post,
                postId);

            reaction = userReaction?.ReactionType.ToString();
        }

        return Result<PostDetailDto>.Success(
            MapToPostDetailDto(post, reaction, tags));
    }

    // ─────────────────────────────
    // PINNED
    // ─────────────────────────────

    public async Task<Result<IReadOnlyList<PostSummaryDto>>> GetPinnedAsync(
        Guid communityId,
        Guid channelId,
        CancellationToken ct = default)
    {
        var posts = await _postRepo.GetPinnedAsync(communityId, channelId, ct);

        if (!posts.Any())
            return Result<IReadOnlyList<PostSummaryDto>>.Success([]);

        var postIds = posts.Select(p => p.Id).ToList();
        var tagLookup = await _tagRepo.GetByTargetsAsync(TagTarget.Post, postIds, ct);

        var dtos = posts.Select(p =>
            MapToPostSummaryDto(
                p,
                null,
                tagLookup[p.Id]))
            .ToList();

        return Result<IReadOnlyList<PostSummaryDto>>.Success(dtos);
    }

    #endregion
    #region update
    // ─────────────────────────────
    // UPDATE
    // ─────────────────────────────

    public async Task<Result<PostDetailDto>> UpdateAsync(
        Guid postId,
        UpdatePostRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        var post = await _postRepo.GetByIdAsync(postId);

        if (post is null || post.IsDeleted)
            return Result<PostDetailDto>.Failure("Post not found.");

        var membership = await _communityRepo.GetMembershipAsync(post.CommunityId, userId, ct);

        if (membership is null)
            return Result<PostDetailDto>.Failure("You must be a member of this community.");

        var channels = await _communityRepo.GetChannelsAsync(post.CommunityId, ct);
        var channel = channels.FirstOrDefault(c => c.Id == post.ChannelId);
        if (channel is null)
            return Result<PostDetailDto>.Failure("Channel not found.");

        if (!_authorization.CanUpdate(post.PostType, channel.ChannelType, membership))
            return Result<PostDetailDto>.Failure("You are not allowed to create this post.");

        var uploadedImages = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Title))
            post.Title = request.Title;

        if (!string.IsNullOrWhiteSpace(request.Content))
            post.Content = request.Content;

        post.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var url in request.RemoveImageUrls)
            {
                if (post.ImageUrls.Remove(url))
                    await _fileStorage.DeleteAsync(url, ct);
            }

            foreach (var image in request.AddImages)
            {
                var upload = await _fileStorage.UploadAsync(image, "posts", ct);

                if (!upload.IsSuccess || upload.Value is null)
                    return Result<PostDetailDto>.Failure($"Image upload failed: {image.FileName}");

                uploadedImages.Add(upload.Value);
            }

            post.ImageUrls.AddRange(uploadedImages);

            if (request.Tags is not null)
                await _tagRepo.ReplaceTagsAsync(TagTarget.Post, postId, request.Tags, ct);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();

            foreach (var url in uploadedImages)
                await _fileStorage.DeleteAsync(url, ct);

            _logger.LogError(ex, "Post update failed");

            return Result<PostDetailDto>.Failure("Error updating post.");
        }

        return await GetByIdAsync(postId, userId, ct);
    }
    #endregion
    #region delete

    // ─────────────────────────────
    // DELETE
    // ─────────────────────────────

    public async Task<Result> DeleteAsync(
        Guid postId,
        Guid userId,
        CancellationToken ct = default)
    {
        var post = await _postRepo.GetByIdAsync(postId);

        if (post is null)
            return Result.Failure("Post not found.");

        var membership = await _communityRepo.GetMembershipAsync(post.CommunityId, userId, ct);

        if (membership is null || post.AuthorId != membership.Id)
            return Result.Failure("You are not allowed to delete this post.");

        post.IsDeleted = true;
        post.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    #endregion
    #region mapping
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

    private static PostDetailDto MapToPostDetailDto(
        Post post,
        string? reaction,
        IEnumerable<Tag> tags)
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

    private static PostSummaryDto MapToPostSummaryDto(
        Post post,
        string? reaction,
        IEnumerable<Tag> tags)
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
    #endregion

}
