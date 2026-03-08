using System.Linq.Expressions;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Author;
using LetopiaPlatform.Core.DTOs.Post;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Exceptions;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Core.Services.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Services;

/// <summary>
/// Provides business logic for managing community posts,
/// including creation, retrieval, and moderation.
/// </summary>
public class PostService : IPostService
{
    private readonly IPostRepository _postRepo;
    private readonly IGenericRepository<Community> _communityRepo;
    private readonly IReactionRepository _reactionRepo;
    private readonly IPostAuthorizationService _postAuthorization;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly ILogger<PostService> _logger;
    private readonly IFileStorageService _fileStorageService;


    public PostService(
        IPostRepository postRepo,
        IGenericRepository<Community> communityRepo,
        IReactionRepository reactionRepo,
        IPostAuthorizationService postAuthorization,
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        ILogger<PostService> logger,
          IFileStorageService fileStorageService)
      
    {
        _postRepo = postRepo;
        _communityRepo = communityRepo;
        _reactionRepo = reactionRepo;
        _postAuthorization = postAuthorization;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    #region Create Post

    public async Task<PostDetailDto> CreateAsync(
        Guid communityId,
        Guid channelId,
        CreatePostRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var community = await _communityRepo.GetByIdAsync(
            communityId,
            "Members"
        );

        if (community == null)
            throw new NotFoundException("Community not found");

        var userCommunity = community.Members?
            .FirstOrDefault(m => m.UserId == userId);

        if (!_postAuthorization.CanCreate(request.PostType, userCommunity!))
            throw new ForbiddenException(
                "You are not allowed to create this type of post.");
        string? postImageUrl = null;
        if (request.PostImage is not null)
        {
            var uploadResult = await _fileStorageService.UploadAsync(request.PostImage, "posts");
            postImageUrl = uploadResult.Value;
        }
      

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var post = new Post
            {
                Title = request.Title,
                Content = request.Content,
                PostType = request.PostType,
                PostImageUrl = postImageUrl,
                AuthorId = userId,
                CommunityId = communityId,
                ChannelId = channelId,
                CreatedAt = DateTime.UtcNow
            };

            await _postRepo.AddAsync(post);

            community.PostCount++;
            await _communityRepo.UpdateAsync(community);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation(
                "Post {PostId} created by User {UserId} in Community {CommunityId}",
                post.Id, userId, communityId);

            return await GetByIdAsync(post.Id, userId, ct);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    #endregion

    #region List Posts

    public async Task<PaginatedResult<PostSummaryDto>> ListAsync(
        Guid communityId,
        Guid channelId,
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var hasSearch = !string.IsNullOrWhiteSpace(search);
        var keyword = hasSearch ? $"%{search!.Trim()}%" : null;

        Expression<Func<Post, bool>> filter = p =>
            p.CommunityId == communityId &&
            p.ChannelId == channelId &&
            !p.IsDeleted &&
            (
                !hasSearch ||
                EF.Functions.Like(p.Title!, keyword!) ||
                EF.Functions.Like(p.Content!, keyword!)
            );

        Expression<Func<Post, object>> orderBy = sortBy?.ToLowerInvariant() switch
        {
            "upvotes" => p => p.Upvotes,
            "comments" => p => p.CommentCount,
            _ => p => p.CreatedAt
        };

        var result = await _postRepo.GetPagedAsync(
            new PaginatedQuery { Page = page, PageSize = pageSize },
            filter,
            orderBy,
            false,
            "Author"
        );

        return new PaginatedResult<PostSummaryDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = result.TotalItems,
            Items = result.Items
                .Select(p => MapToPostSummaryDto(p, null))
                .ToList()
        };
    }

    #endregion

    #region Get Post

    public async Task<PostDetailDto> GetByIdAsync(
        Guid postId,
        Guid? currentUserId = null,
        CancellationToken ct = default)
    {
        var post = await _postRepo.GetByIdAsync(postId, "Author");
        if (post == null || post.IsDeleted)
            throw new NotFoundException("Post not found");

        post.ViewsCount++;
        await _unitOfWork.SaveChangesAsync(ct);

        string? reaction = null;
        if (currentUserId.HasValue)
        {
            var userReaction = await _reactionRepo
                .GetUserReactionAsync(currentUserId.Value, TargetType.Post, post.Id);

            reaction = userReaction?.ReactionType.ToString();
        }

        return MapToPostDetailDto(post, reaction);
    }

    #endregion

    #region Update Post

    public async Task<PostDetailDto> UpdateAsync(
        Guid postId,
        UpdatePostRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        var post = await _postRepo.GetByIdAsync(
            postId,
            "Community.Members",
            "Author"
        );

        if (post == null || post.IsDeleted)
            throw new NotFoundException("Post not found");

        var userCommunity = post.Community?.Members?
            .FirstOrDefault(m => m.UserId == userId);

        if (post.AuthorId != userId &&
            !_postAuthorization.CanUpdate(post.PostType, userCommunity!))
            throw new ForbiddenException(
                "You are not allowed to update this post.");

        if (!string.IsNullOrWhiteSpace(request.Title))
            post.Title = request.Title;

        if (!string.IsNullOrWhiteSpace(request.Content))
            post.Content = request.Content;
 
        if (request.PostImage is not null)
        {
            var replaceResult = await _fileStorageService.ReplaceAsync(request.PostImage, "posts", post.PostImageUrl);
            post.PostImageUrl = replaceResult.Value;
        }

        post.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(post.Id, userId, ct);
    }

    #endregion

    #region Delete Post

    public async Task DeleteAsync(
        Guid postId,
        Guid userId,
        CancellationToken ct = default)
    {
        var post = await _postRepo.GetByIdAsync(
            postId,
            "Community.Members"
        );

        if (post == null || post.IsDeleted)
            throw new NotFoundException("Post not found");

        var userCommunity = post.Community?.Members?
            .FirstOrDefault(m => m.UserId == userId);

        if (post.AuthorId != userId &&
            !_postAuthorization.CanDelete(post.PostType, userCommunity!))
            throw new ForbiddenException(
                "You are not allowed to delete this post.");

        post.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Post {PostId} deleted by User {UserId}",
            postId, userId);
    }

    #endregion

    #region Mapping

    private static AuthorDto MapAuthor(Post post)
    {
        return post.Author != null && !string.IsNullOrEmpty(post.Author.FullName) ?
            new AuthorDto(post.Author.Id, post.Author.FullName, post.Author.AvatarUrl) :
            new AuthorDto(post.AuthorId, "Unknown Author", null); }

    private static PostDetailDto MapToPostDetailDto(Post post, string? reaction)
        => new(
            post.Id,
            post.Title ?? string.Empty,
            post.Content ?? string.Empty,
            post.PostImageUrl ?? string.Empty,
            MapAuthor(post),
            post.PostType,
            post.Upvotes,
            post.CommentCount,
            post.ViewsCount,
            post.IsPinned,
            post.CreatedAt,
            reaction,
            post.UpdatedAt
        );

    private static PostSummaryDto MapToPostSummaryDto(Post post, string? reaction)
    {
        var summary = post.Content?.Length > 200
            ? post.Content[..200] + "..."
            : post.Content ?? string.Empty;

        return new PostSummaryDto(
            post.Id,
            post.Title ?? string.Empty,
            post.PostImageUrl ?? string.Empty,
            summary,
            MapAuthor(post),
            post.PostType,
            post.Upvotes,
            post.CommentCount,
            post.ViewsCount,
            post.IsPinned,
            post.CreatedAt,
            reaction
        );
    }

    #endregion
}
