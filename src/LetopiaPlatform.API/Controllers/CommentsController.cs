using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Core.Reaction;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Comment;
using LetopiaPlatform.Core.DTOs.Reaction;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;

[ApiController]
[Authorize]
public class CommentsController : BaseController
{
    private readonly ICommentService _commentService;
    private readonly IReactionService _reactionService;

    public CommentsController(ICommentService commentService, IReactionService reactionService)
    {
        _commentService = commentService;
        _reactionService = reactionService;
    }

    // ── UPDATE Comment ─────────────────────────────────────────────
    /// <summary>
    /// Update a comment (author only)
    /// </summary>
    [HttpPut(Router.Comments.Update)]
    
    public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateCommentRequest request)
    {
        HttpContext.AddBusinessContext("action", "update_comment");
        HttpContext.AddBusinessContext("comment_id", commentId);

        var userId = GetUserId();
        var updatedComment = await _commentService.UpdateAsync(commentId, request, userId, HttpContext.RequestAborted);
        return HandleResult(Result<CommentDto>.Success(updatedComment));
    }

    // ── DELETE Comment ─────────────────────────────────────────────
    /// <summary>
    /// Delete a comment (author or moderator)
    /// </summary>
    [HttpDelete(Router.Comments.Delete)]
   
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        HttpContext.AddBusinessContext("action", "delete_comment");
        HttpContext.AddBusinessContext("comment_id", commentId);

        var userId = GetUserId();
        await _commentService.DeleteAsync(commentId, userId, HttpContext.RequestAborted);
        return HandleResult(Result.Success());
    }

    // ── TOGGLE Comment Reaction ─────────────────────────────────────
    /// <summary>
    /// Toggle a reaction on a comment (auth required)
    /// </summary>
    [HttpPost(Router.Comments.React)]
  
    public async Task<IActionResult> ToggleCommentReaction(Guid commentId, [FromBody] ToggleReactionRequest request)
    {
        HttpContext.AddBusinessContext("action", "toggle_comment_reaction");
        HttpContext.AddBusinessContext("comment_id", commentId);

        var userId = GetUserId();
        var reactionResult = await _reactionService.ToggleAsync("Comment", commentId, request, userId, HttpContext.RequestAborted);
        return HandleResult(Result<ReactionResultDto>.Success(reactionResult));
    }
}
