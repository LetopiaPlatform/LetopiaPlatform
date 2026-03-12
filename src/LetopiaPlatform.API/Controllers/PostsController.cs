using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Core.Reaction;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Comment;
using LetopiaPlatform.Core.DTOs.Post;
using LetopiaPlatform.Core.DTOs.Reaction;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
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
    // ── Create Discussion ───────────────────────────────────────────────
    /// <summary>
    /// Create a new post in the specified community
    /// </summary>
    [HttpPost(Router.Posts.Create)]

    public async Task<IActionResult> CreatePost(
        Guid communityId,
        Guid channelId,
        [FromForm] CreatePostRequest request)
    {
        HttpContext.AddBusinessContext("action", "create_discussion");
        HttpContext.AddBusinessContext("community_id", communityId);

        var userId = GetUserId();
        var postDetail = await _postService.CreateAsync(communityId, channelId, request, userId, HttpContext.RequestAborted);

        // Wrap in Result<T> to match HandleResult
        return HandleResult(Result<PostDetailDto>.Success(postDetail));
    }

    // ── List post ────────────────────────────────────────────────
    /// <summary>
    /// List all posts in the specified community
    /// </summary>
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
        HttpContext.AddBusinessContext("action", "list_discussions");
        HttpContext.AddBusinessContext("community_id", communityId);

        var paginated = await _postService.ListAsync(
            communityId,
            channelId,
            page,
            pageSize,
            search,
            sortBy,
            HttpContext.RequestAborted
        );

        return HandleResult(
            Result<PaginatedResult<PostSummaryDto>>.Success(paginated)
        );
    }

    // ── GET Post By Id ───────────────────────────────────────────────
    [HttpGet(Router.Posts.GetById)]
    [AllowAnonymous]
    public async Task<IActionResult> GetPost(Guid postId)
    {
        HttpContext.AddBusinessContext("action", "get_post");
        HttpContext.AddBusinessContext("post_id", postId);

        var userId = GetUserId();
        var postDetail = await _postService.GetByIdAsync(postId, userId, HttpContext.RequestAborted);
        return HandleResult(Result<PostDetailDto>.Success(postDetail));
    }

    // ── UPDATE Post ────────────────────────────────────────────────
    [HttpPut(Router.Posts.Update)]
    
    public async Task<IActionResult> UpdatePost(Guid postId, [FromForm] UpdatePostRequest request)
    {
        HttpContext.AddBusinessContext("action", "update_post");
        HttpContext.AddBusinessContext("post_id", postId);

        var userId = GetUserId();
        var updatedPost = await _postService.UpdateAsync(postId, request, userId, HttpContext.RequestAborted);
        return HandleResult(Result<PostDetailDto>.Success(updatedPost));
    }

    // ── DELETE Post ────────────────────────────────────────────────
    [HttpDelete(Router.Posts.Delete)]
   
    public async Task<IActionResult> DeletePost(Guid postId)
    {
        HttpContext.AddBusinessContext("action", "delete_post");
        HttpContext.AddBusinessContext("post_id", postId);

        var userId = GetUserId();
        await _postService.DeleteAsync(postId, userId, HttpContext.RequestAborted);
        return HandleResult(Result.Success());
    }

    // ── CREATE Comment ─────────────────────────────────────────────
    [HttpPost(Router.Posts.Comments)]

    public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CreateCommentRequest request)
    {
        HttpContext.AddBusinessContext("action", "create_comment");
        HttpContext.AddBusinessContext("post_id", postId);

        var userId = GetUserId();
        var comment = await _commentService.CreateAsync(postId, request, userId, HttpContext.RequestAborted);
        return HandleResult(Result<CommentDto>.Success(comment));
    }

    // ── LIST Comments ──────────────────────────────────────────────
    [HttpGet(Router.Posts.Comments)]
    [AllowAnonymous]
    public async Task<IActionResult> ListComments(
     Guid postId,
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 50,
     [FromQuery] string? search = null)
    {
        // Add business context for logging/tracking
        HttpContext.AddBusinessContext("action", "list_comments");
        HttpContext.AddBusinessContext("post_id", postId);

        // Get current user ID (if logged in)
        var currentUserId = GetUserId();

        // Call service with pagination, search, and optional user reactions
        var comments = await _commentService.ListAsync(
            postId,
            page,
            pageSize,
            search,
            currentUserId,
            HttpContext.RequestAborted
        );

        // Wrap result in a standard response
        return HandleResult(Result<PaginatedResult<CommentDto>>.Success(comments));
    }


    // ── TOGGLE Reaction ───────────────────────────────────────────
    [HttpPost(Router.Posts.React)]

    public async Task<IActionResult> ToggleReaction(Guid postId, [FromBody] ToggleReactionRequest request)
    {
        HttpContext.AddBusinessContext("action", "toggle_post_reaction");
        HttpContext.AddBusinessContext("post_id", postId);

        var userId = GetUserId();
        var reactionResult = await _reactionService.ToggleAsync(TargetType.Post, postId, request, userId, HttpContext.RequestAborted);
        return HandleResult(Result<ReactionResultDto>.Success(reactionResult));
    }
}
