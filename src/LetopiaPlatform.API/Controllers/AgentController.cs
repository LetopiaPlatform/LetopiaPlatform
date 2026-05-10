using System.IO;
using System.Text.Json;
using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Common;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LetopiaPlatform.API.Controllers;

[Authorize]
public class AgentController : BaseController
{
    private readonly IRoadmapAgentService _agentService;
    private readonly ILogger<AgentController> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AgentController(
        IRoadmapAgentService agentService,
        ILogger<AgentController> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    /// <summary>
    /// Start a new agent conversation and stream the initial response via SSE.
    /// </summary>
    [HttpPost(Router.Agent.Conversations)]
    [EnableRateLimiting(RateLimitingExtensions.AiGenerationPolicy)]
    [ProducesResponseType(typeof(AgentStreamEvent), StatusCodes.Status200OK, "text/event-stream")]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task StartConversation(
        [FromBody] StartConversationRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();

        HttpContext.AddBusinessContext("action", "start_agent_conversation");

        AgentConversation conversation;

        try
        {
            conversation = await _agentService.StartConversationAsync(
                userId, request.InitialMessage, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start conversation for user {UserId}", userId);

            Response.StatusCode = StatusCodes.Status500InternalServerError;
            Response.ContentType = "application/json";
            await Response.WriteAsJsonAsync(new ErrorResponse
            {
                Status = 500,
                Message = "Failed to start conversation."
            }, ct);
            return;
        }

        HttpContext.AddBusinessContext("conversation_id", conversation.Id);

        await StreamSseAsync(conversation.Id, request.InitialMessage, userId, ct);
    }

    /// <summary>
    /// Send a message to an existing conversation and stream the response via SSE.
    /// </summary>
    [HttpPost(Router.Agent.Messages)]
    [EnableRateLimiting(RateLimitingExtensions.AiChatPolicy)]
    [ProducesResponseType(typeof(AgentStreamEvent), StatusCodes.Status200OK, "text/event-stream")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task SendMessage(
        Guid conversationId,
        [FromBody] SendMessageRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();

        HttpContext.AddBusinessContext("action", "send_agent_message");
        HttpContext.AddBusinessContext("conversation_id", conversationId);

        // Throws NotFoundException / ForbiddenException — handled by ExceptionMiddleware
        await _agentService.ValidateConversationOwnershipAsync(conversationId, userId, ct);

        await StreamSseAsync(conversationId, request.Content, userId, ct);
    }

    /// <summary>
    /// List all conversations for the authenticated user.
    /// </summary>
    [HttpGet(Router.Agent.Conversations)]
    [ProducesResponseType(typeof(ApiResponse<List<ConversationSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListConversations(CancellationToken ct)
    {
        var userId = GetUserId();

        HttpContext.AddBusinessContext("action", "list_agent_conversations");

        var dtos = await _agentService.GetUserConversationsAsync(userId, ct);

        return Ok(ApiResponse<List<ConversationSummaryDto>>.SuccessResponse(dtos));
    }

    /// <summary>
    /// Get a conversation with all its messages.
    /// </summary>
    [HttpGet(Router.Agent.ConversationById)]
    [ProducesResponseType(typeof(ApiResponse<ConversationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConversation(Guid conversationId, CancellationToken ct)
    {
        var userId = GetUserId();

        HttpContext.AddBusinessContext("action", "get_agent_conversation");
        HttpContext.AddBusinessContext("conversation_id", conversationId);

        // Throws NotFoundException / ForbiddenException — handled by ExceptionMiddleware
        var dto = await _agentService.GetConversationAsync(conversationId, userId, ct);

        return Ok(ApiResponse<ConversationDto>.SuccessResponse(dto));
    }

    /// <summary>
    /// Configures SSE headers and streams agent events to the client.
    /// </summary>
    private async Task StreamSseAsync(Guid conversationId, string message, Guid userId, CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";
        Response.Headers["X-Accel-Buffering"] = "no";

        try
        {
            await foreach (var ev in _agentService.ProcessMessageAsync(conversationId, message, userId, ct))
            {
                var data = JsonSerializer.Serialize(ev.Data, JsonOptions);
                await Response.WriteAsync($"event: {ev.Type}\ndata: {data}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Client disconnected — no action needed.
            _logger.LogInformation("SSE stream cancelled for conversation {ConversationId}", conversationId);
        }
        catch (IOException ex)
        {
            if (ct.IsCancellationRequested)
            {
                // Almost certainly a clean client disconnect — not worth alerting on
                _logger.LogInformation(
                    "Client disconnected mid-stream for {ConversationId}",
                    conversationId);
                return;
            }

            // Real IO problem — let it surface
            _logger.LogError(ex, "Unexpected IO error for {ConversationId}", conversationId);
            throw;
        }
    }
}
