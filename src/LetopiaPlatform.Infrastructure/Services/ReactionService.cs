
using LetopiaPlatform.API.Core.Reaction;
using LetopiaPlatform.Core.DTOs.Reaction;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Services;

/// <summary>
/// Provides business logic for managing reactions on posts and comments,
/// including adding, removing, and toggling reaction types.
/// </summary>
public class ReactionService : IReactionService
{
    private readonly IReactionRepository _reactionRepo;
    private readonly IPostRepository _postRepo;
    private readonly ICommentRepository _commentRepo;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly ILogger<ReactionService> _logger;

    public ReactionService(
        IReactionRepository reactionRepo,
        IPostRepository postRepo,
        ICommentRepository commentRepo,
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        ILogger<ReactionService> logger)
    {
        _reactionRepo = reactionRepo ?? throw new ArgumentNullException(nameof(reactionRepo));
        _postRepo = postRepo ?? throw new ArgumentNullException(nameof(postRepo));
        _commentRepo = commentRepo ?? throw new ArgumentNullException(nameof(commentRepo));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Toggles a reaction for a specific target entity (post or comment).
    /// </summary>
    public async Task<ReactionResultDto> ToggleAsync(
         TargetType targetType,
        Guid targetId,
        ToggleReactionRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (targetType == TargetType.Post)
        {
            var post = await _postRepo.GetByIdAsync(targetId);
            if (post == null || post.IsDeleted)
                throw new KeyNotFoundException("Post not found.");

            return await ToggleReactionForTarget(postId: post.Id, commentId: null, targetType: TargetType.Post, request.ReactionType, userId, ct);
        }
        else if (targetType == TargetType.Comment)
        {
            var comment = await _commentRepo.GetByIdAsync(targetId);
            if (comment == null || comment.IsDeleted)
                throw new KeyNotFoundException("Comment not found.");

            return await ToggleReactionForTarget(postId: comment.PostId, commentId: comment.Id, targetType: TargetType.Comment, request.ReactionType, userId, ct);
        }
        else
        {
            throw new ArgumentException("Invalid target type. Must be 'Post' or 'Comment'.", nameof(targetType));
        }
    }

    private async Task<ReactionResultDto> ToggleReactionForTarget(
        Guid postId,
        Guid? commentId,
        TargetType targetType,
        ReactionType newReaction,
        Guid userId,
        CancellationToken ct)
    {
        var targetId = commentId ?? postId;

        // Check for existing reaction
        var existing = (await _reactionRepo.FindAsync(r =>
            r.UserId == userId &&
            r.TargetType == targetType &&
            r.TargetId == targetId)).FirstOrDefault();

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (existing == null)
            {
                await _reactionRepo.AddAsync(new Reaction
                {
                    UserId = userId,
                    TargetType = targetType,
                    TargetId = targetId,
                    ReactionType = newReaction,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (existing.ReactionType == newReaction)
            {
                await _reactionRepo.DeleteAsync(existing);
                existing = null;
            }
            else
            {
                existing.ReactionType = newReaction;
                await _reactionRepo.UpdateAsync(existing);
            }

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync();

            // Recalculate reaction counts
            var reactions = await _reactionRepo.FindAsync(r => r.TargetType == targetType && r.TargetId == targetId);
            var upvotes = reactions.Count(r => r.ReactionType == ReactionType.Upvote);
           

            // Update entity counters
            if (targetType == TargetType.Post)
            {
                var post = await _postRepo.GetByIdAsync(postId);
                if (post != null) post.Upvotes = upvotes ;
                await _unitOfWork.SaveChangesAsync(ct);
            }
            else if (targetType == TargetType.Comment && commentId.HasValue)
            {
                var comment = await _commentRepo.GetByIdAsync(commentId.Value);
                if (comment != null) comment.Upvotes = upvotes;
                await _unitOfWork.SaveChangesAsync(ct);
            }

            return new ReactionResultDto(
                CurrentReaction: existing?.ReactionType.ToString(),
                Upvotes: upvotes
              
            );
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Failed to toggle reaction for {TargetType} {TargetId}", targetType, targetId);
            throw;
        }
    }
}
