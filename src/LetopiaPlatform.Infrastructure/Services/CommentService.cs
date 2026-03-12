using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Author;
using LetopiaPlatform.Core.DTOs.Comment;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Exceptions;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Services;

/// <summary>
/// Provides operations for managing comments, including creation, retrieval, update, and soft-deletion.
/// Supports polymorphic reactions through TargetType and TargetId mapping.
/// </summary>
public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepo;
    private readonly IPostRepository _postRepo;
    private readonly IReactionRepository _reactionRepo;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly ILogger<CommentService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentService"/> class.
    /// </summary>
    public CommentService(
        ICommentRepository commentRepo,
        IReactionRepository reactionRepo,
        IPostRepository postRepo,
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        ILogger<CommentService> logger)
    {
        _commentRepo = commentRepo ?? throw new ArgumentNullException(nameof(commentRepo));
        _reactionRepo = reactionRepo ?? throw new ArgumentNullException(nameof(reactionRepo));
        _postRepo = postRepo ?? throw new ArgumentNullException(nameof(postRepo));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Create

    public async Task<CommentDto> CreateAsync(Guid postId, CreateCommentRequest request, Guid userId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var post = await _postRepo.GetByIdAsync(postId, "Community.Members");
        if (post == null || post.IsDeleted)
            throw new NotFoundException("Post not found.");

        if (post.PostType != PostType.Discussion)
            throw new InvalidOperationException("Comments are only allowed on discussion posts.");

        var memberRole = post.Community?.Members?.FirstOrDefault(m => m.UserId == userId)?.Role;
        if (memberRole != CommunityRole.Member &&
            memberRole != CommunityRole.Moderator &&
            memberRole != CommunityRole.Owner)
        {
            throw new ForbiddenException("User does not have permission to create comment.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var comment = new Comment
            {
                PostId = postId,
                AuthorId = userId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepo.AddAsync(comment);
            post.CommentCount++;

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Comment {CommentId} created by User {UserId}", comment.Id, userId);
            return await GetByIdInternalAsync(comment.Id, userId, ct);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Transaction failed: Could not create comment for Post {PostId}", postId);
            throw;
        }
    }

    #endregion

    #region List

    public async Task<PaginatedResult<CommentDto>> ListAsync(
        Guid postId,
        int page = 1,
        int pageSize = 50,
        string? search = null,
        Guid? currentUserId = null,
        CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 || pageSize > 100 ? 50 : pageSize;

        var result = await _commentRepo.GetCommentsByPostIdAsync(postId, page, pageSize);

        if (result?.Items == null || result.Items.Count == 0)
            return new PaginatedResult<CommentDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = 0,
                Items = new List<CommentDto>()
            };

        var userReactions = new Dictionary<Guid, string>();
        if (currentUserId.HasValue)
        {
            var commentIds = result.Items.Select(c => c.Id).ToList();
            foreach (var id in commentIds)
            {
                var reaction = await _reactionRepo.GetUserReactionAsync(currentUserId.Value,TargetType.Comment, id);
                if (reaction != null) userReactions[id] = reaction.ReactionType.ToString();
            }
        }

        var items = result.Items
            .Select(c => MapToCommentDto(c, userReactions.GetValueOrDefault(c.Id)))
            .ToList();

        return new PaginatedResult<CommentDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = result.TotalItems,
            Items = items
        };
    }

    #endregion

    #region Update

    public async Task<CommentDto> UpdateAsync(Guid commentId, UpdateCommentRequest request, Guid userId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var comment = await _commentRepo.GetByIdAsync(commentId, "Author","Post.Community.Members");
        if (comment == null || comment.IsDeleted) throw new NotFoundException("Comment not found.");

        var memberRole = comment.Post?.Community?.Members?.FirstOrDefault(m => m.UserId == userId)?.Role;
        if (comment.AuthorId != userId && memberRole != CommunityRole.Moderator)
            throw new ForbiddenException("Only the author or a moderator can update this comment.");

        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);

        var reaction = await _reactionRepo.GetUserReactionAsync(userId, TargetType.Comment, commentId);
        return MapToCommentDto(comment, reaction?.ReactionType.ToString());
    }

    #endregion

    #region Delete

    public async Task DeleteAsync(Guid commentId, Guid userId, CancellationToken ct = default)
    {
        var comment = await _commentRepo.GetByIdAsync(commentId, "Post.Community.Members");
        if (comment == null || comment.IsDeleted) throw new NotFoundException("Comment not found.");

        var memberRole = comment?.Post?.Community?.Members?.FirstOrDefault(m => m.UserId == userId)?.Role;
        bool canDelete = comment?.AuthorId == userId || memberRole == CommunityRole.Moderator;

        if (!canDelete) throw new ForbiddenException("Insufficient permissions to delete this comment.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (comment != null) comment.IsDeleted = true;
            if (comment?.Post != null)
                comment.Post.CommentCount = Math.Max(0, comment.Post.CommentCount - 1);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Comment {CommentId} soft-deleted by User {UserId}", commentId, userId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Failed to delete comment {CommentId}", commentId);
            throw;
        }
    }

    #endregion

    #region Helpers

    private async Task<CommentDto> GetByIdInternalAsync(Guid commentId, Guid? currentUserId, CancellationToken ct)
    {
        var comment = await _commentRepo.GetByIdAsync(commentId, "Author");
        if (comment == null) throw new NotFoundException("Comment not found.");

        string? reactionType = null;
        if (currentUserId.HasValue)
        {
            var reaction = await _reactionRepo.GetUserReactionAsync(currentUserId.Value, TargetType.Comment, commentId);
            reactionType = reaction?.ReactionType.ToString();
        }

        return MapToCommentDto(comment, reactionType);
    }

    private static AuthorDto MapAuthor(Comment comment)
    {
        return comment.Author != null && !string.IsNullOrEmpty(comment.Author.FullName)
            ? new AuthorDto(comment.Author.Id, comment.Author.FullName, comment.Author.AvatarUrl)
            : new AuthorDto(comment.AuthorId, "Unknown Author", null);
    }

    private static CommentDto MapToCommentDto(Comment comment, string? currentUserReaction)
    {
        return new CommentDto(
            comment.Id,
            comment.PostId,
            MapAuthor(comment),
            comment.Content ?? string.Empty,
            comment.Upvotes,
            comment.CreatedAt,
            comment.UpdatedAt,
            currentUserReaction
        );
    }

    #endregion
}
