using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Core.Reaction;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Comment;
using LetopiaPlatform.Core.DTOs.Post;
using LetopiaPlatform.Core.DTOs.Reaction;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;

[ApiController]
[Authorize]
public class PostsController : BaseController
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly IReactionService _reactionService;

    public PostsController(
        IPostService postService,
        ICommentService commentService,
        IReactionService reactionService)
    {
        _postService = postService;
        _commentService = commentService;
        _reactionService = reactionService;
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [HttpPost(Router.Posts.Create)]
    public async Task<IActionResult> CreatePost(
        Guid communityId,
        Guid channelId,
        [FromForm] CreatePostRequest request)
    {
        HttpContext.AddBusinessContext("action", "create_post");
        HttpContext.AddBusinessContext("community_id", communityId.ToString());
        HttpContext.AddBusinessContext("channel_id", channelId.ToString());

        var userId = GetUserId();
        var post = await _postService.CreateAsync(communityId, channelId, request, userId, HttpContext.RequestAborted);
        return HandleResult(post);
    }

    // ── List ──────────────────────────────────────────────────────────────────

    [HttpGet(Router.Posts.List)]
    [AllowAnonymous]
    public async Task<IActionResult> ListPosts(
        Guid communityId,
        Guid channelId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null)
    {
        HttpContext.AddBusinessContext("action", "list_posts");
        HttpContext.AddBusinessContext("community_id", communityId.ToString());
        HttpContext.AddBusinessContext("channel_id", channelId.ToString());

        var paginated = await _postService.ListAsync(
            communityId, channelId, page, pageSize, search, sortBy,
            HttpContext.RequestAborted);

        return HandleResult(paginated);
    }

    // ── Pinned ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all pinned posts for a channel.
    /// Rendered above the regular post feed — no pagination.
    /// </summary>
    [HttpGet(Router.Posts.Pinned)]
    [AllowAnonymous]
    public async Task<IActionResult> GetPinnedPosts(Guid communityId, Guid channelId)
    {
        HttpContext.AddBusinessContext("action", "get_pinned_posts");
        HttpContext.AddBusinessContext("community_id", communityId.ToString());
        HttpContext.AddBusinessContext("channel_id", channelId.ToString());

        var pinned = await _postService.GetPinnedAsync(communityId, channelId, HttpContext.RequestAborted);
        return HandleResult(pinned);
    }

    // ── Get by ID ─────────────────────────────────────────────────────────────

    [HttpGet(Router.Posts.GetById)]
    [AllowAnonymous]
    public async Task<IActionResult> GetPost(Guid postId)
    {
        HttpContext.AddBusinessContext("action", "get_post");
        HttpContext.AddBusinessContext("post_id", postId.ToString());

        var userId = GetUserId();
        var post = await _postService.GetByIdAsync(postId, userId, HttpContext.RequestAborted);
        return HandleResult(post);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [HttpPut(Router.Posts.Update)]
    public async Task<IActionResult> UpdatePost(Guid postId, [FromForm] UpdatePostRequest request)
    {
        HttpContext.AddBusinessContext("action", "update_post");
        HttpContext.AddBusinessContext("post_id", postId.ToString());

        var userId = GetUserId();
        var updatedPost = await _postService.UpdateAsync(postId, request, userId, HttpContext.RequestAborted);
        return HandleResult(updatedPost);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [HttpDelete(Router.Posts.Delete)]
    public async Task<IActionResult> DeletePost(Guid postId)
    {
        HttpContext.AddBusinessContext("action", "delete_post");
        HttpContext.AddBusinessContext("post_id", postId.ToString());

        var userId = GetUserId();
        await _postService.DeleteAsync(postId, userId, HttpContext.RequestAborted);
        return HandleResult(Result.Success());
    }

    // ── Comments ──────────────────────────────────────────────────────────────

    [HttpPost(Router.Posts.Comments)]
    public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CreateCommentRequest request)
    {
        HttpContext.AddBusinessContext("action", "create_comment");
        HttpContext.AddBusinessContext("post_id", postId.ToString());

        var userId = GetUserId();
        var comment = await _commentService.CreateAsync(postId, request, userId, HttpContext.RequestAborted);
        return HandleResult(Result<CommentDto>.Success(comment));
    }

    [HttpGet(Router.Posts.Comments)]
    [AllowAnonymous]
    public async Task<IActionResult> ListComments(
        Guid postId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null)
    {
        HttpContext.AddBusinessContext("action", "list_comments");
        HttpContext.AddBusinessContext("post_id", postId.ToString());

        var userId = GetUserId();
        var comments = await _commentService.ListAsync(
            postId, page, pageSize, search, userId,
            HttpContext.RequestAborted);

        return HandleResult(Result<PaginatedResult<CommentDto>>.Success(comments));
    }

    // ── Reaction ──────────────────────────────────────────────────────────────

    [HttpPost(Router.Posts.React)]
    public async Task<IActionResult> ToggleReaction(Guid postId, [FromBody] ToggleReactionRequest request)
    {
        HttpContext.AddBusinessContext("action", "toggle_post_reaction");
        HttpContext.AddBusinessContext("post_id", postId.ToString());

        var userId = GetUserId();
        var reactionResult = await _reactionService.ToggleAsync(
            TargetType.Post, postId, request, userId,
            HttpContext.RequestAborted);

        return HandleResult(Result<ReactionResultDto>.Success(reactionResult));
    }
}
