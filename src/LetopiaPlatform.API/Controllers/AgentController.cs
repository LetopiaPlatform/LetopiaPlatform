using System.IO;
using System.Text.Json;
using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Common;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LetopiaPlatform.API.Controllers;

[Authorize]
public class AgentController : BaseController
{
    private readonly IRoadmapAgentService _agentService;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly ILogger<AgentController> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AgentController(
        IRoadmapAgentService agentService,
        IConversationRepository conversationRepository,
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        ILogger<AgentController> logger)
    {
        _agentService = agentService;
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Start a new agent conversation and stream the initial response via SSE.
    /// </summary>
    [HttpPost(Router.Agent.Conversations)]
    [EnableRateLimiting(RateLimitingExtensions.AiGenerationPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task StartConversation(
        [FromBody] StartConversationRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();

        HttpContext.AddBusinessContext("action", "start_agent_conversation");

        var conversation = await _agentService.StartConversationAsync(userId, request.InitialMessage, ct);

        HttpContext.AddBusinessContext("conversation_id", conversation.Id);

        await StreamSseAsync(conversation.Id, request.InitialMessage, userId, ct);
    }

    /// <summary>
    /// Send a message to an existing conversation and stream the response via SSE.
    /// </summary>
    [HttpPost(Router.Agent.Messages)]
    [EnableRateLimiting(RateLimitingExtensions.AiChatPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        var conversation = await _conversationRepository.GetByIdAsync(conversationId, ct);

        if (conversation is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            Response.ContentType = "application/json";
            await Response.WriteAsJsonAsync(new ErrorResponse
            {
                Status = 404,
                Message = "Conversation not found."
            }, ct);
            return;
        }

        if (conversation.UserId != userId)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            Response.ContentType = "application/json";
            await Response.WriteAsJsonAsync(new ErrorResponse
            {
                Status = 403,
                Message = "You do not have access to this conversation."
            }, ct);
            return;
        }

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

        var conversations = await _conversationRepository.GetByUserIdAsync(userId, ct);

        var dtos = conversations
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => new ConversationSummaryDto(
                c.Id,
                c.Title,
                c.AgentType,
                c.Status,
                c.CreatedAt,
                c.UpdatedAt))
            .ToList();

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

        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, ct);

        if (conversation is null)
            return NotFound(new ErrorResponse { Status = 404, Message = "Conversation not found." });

        if (conversation.UserId != userId)
            return StatusCode(StatusCodes.Status403Forbidden,
                new ErrorResponse { Status = 403, Message = "You do not have access to this conversation." });

        var dto = new ConversationDto(
            conversation.Id,
            conversation.Title,
            conversation.AgentType,
            conversation.Status,
            conversation.RoadmapId,
            conversation.CreatedAt,
            conversation.UpdatedAt,
            conversation.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ConversationMessageDto(m.Id, m.Role, m.Content, m.CreatedAt))
                .ToList());

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
        catch (Exception ex) when (ex is IOException or ObjectDisposedException)
        {
            _logger.LogError(ex, "SSE stream error for conversation {ConversationId}", conversationId);

            // Attempt to send an error event to the client before closing.
            try
            {
                var errorData = JsonSerializer.Serialize(new { message = "An internal error occurred." }, JsonOptions);
                await Response.WriteAsync($"event: error\ndata: {errorData}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
            catch (IOException)
            {
                // Connection dropped while writing error event — swallow.
            }
            catch (ObjectDisposedException)
            {
                // Response stream already disposed — swallow.
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // Response cancelled during error write — swallow.
            }
        }
    }
}
