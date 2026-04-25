using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using LetopiaPlatform.Agent.Prompts;
using LetopiaPlatform.Agent.Tools;
using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Exceptions;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Agent.Services;

public class RoadmapAgentService : IRoadmapAgentService
{
    private const int MaxIterations = 10;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IChatClient _chatClient;
    private readonly IWebSearchService _webSearchService;
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RoadmapAgentService> _logger;

    public RoadmapAgentService(
        IChatClient chatClient,
        IWebSearchService webSearchService,
        IRoadmapRepository roadmapRepository,
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        ILogger<RoadmapAgentService> logger)
    {
        _chatClient = chatClient;
        _webSearchService = webSearchService;
        _roadmapRepository = roadmapRepository;
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public AgentType AgentType => AgentType.RoadmapGenerator;

    public async Task<AgentConversation> StartConversationAsync(
        Guid userId,
        string initialMessage,
        CancellationToken ct)
    {
        var conversation = new AgentConversation
        {
            UserId = userId,
            AgentType = AgentType.RoadmapGenerator,
            Status = ConversationStatus.Active,
            Title = initialMessage.Length > 100 ? initialMessage[..100] : initialMessage
        };

        _conversationRepository.Add(conversation);

        _conversationRepository.AddMessage(new ConversationMessage
        {
            ConversationId = conversation.Id,
            Role = MessageRole.System,
            Content = PromptLoader.RoadmapSystemPrompt,
            CreatedAt = DateTime.UtcNow
        });

        _conversationRepository.AddMessage(new ConversationMessage
        {
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = initialMessage,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync(ct);
        return conversation;
    }

    public async IAsyncEnumerable<AgentStreamEvent> ProcessMessageAsync(
        Guid conversationId,
        string userMessage,
        Guid userId,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, ct)
            ?? throw new NotFoundException(nameof(AgentConversation), conversationId);

        if (conversation.UserId != userId)
            throw new ForbiddenException();

        _conversationRepository.AddMessage(new ConversationMessage
        {
            ConversationId = conversationId,
            Role = MessageRole.User,
            Content = userMessage,
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync(ct);

        var messages = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(ToChatMessage)
            .ToList();
        messages.Add(new ChatMessage(ChatRole.User, userMessage));

        var searchTool = WebSearchTool.Create(_webSearchService);
        var options = new ChatOptions { Tools = [searchTool] };

        // C# forbids yield inside catch blocks. The agent loop is delegated to
        // RunAgentLoopAsync (a regular async Task method) which collects all events
        // and errors. We then replay them here, yielding outside any catch block.
        var result = await RunAgentLoopAsync(
            conversation, conversationId, messages, searchTool, options, ct);

        if (result.Cancelled)
            yield break;

        foreach (var ev in result.Events)
            yield return ev;

        if (result.Error is not null)
            yield return new AgentStreamEvent("error", result.Error);
    }

    /// <summary>
    /// Runs the agent loop non-iteratively, collecting all events into a list.
    /// This is required because C# forbids yield inside catch blocks.
    /// Real-time delta streaming is sacrificed in the error-boundary layer;
    /// all deltas are buffered and replayed. The normal (no-exception) path
    /// still produces the full event sequence.
    /// </summary>
    private async Task<AgentLoopResult> RunAgentLoopAsync(
        AgentConversation conversation,
        Guid conversationId,
        List<ChatMessage> messages,
        AIFunction searchTool,
        ChatOptions options,
        CancellationToken ct)
    {
        var events = new List<AgentStreamEvent>();

        try
        {
            for (int i = 0; i < MaxIterations; i++)
            {
                var textBuffer = new StringBuilder();
                var toolCalls = new List<FunctionCallContent>();

                await foreach (var update in _chatClient.CompleteStreamingAsync(messages, options, ct))
                {
                    if (update.Text is not null)
                    {
                        textBuffer.Append(update.Text);
                        events.Add(new AgentStreamEvent("delta", update.Text));
                    }

                    toolCalls.AddRange(update.Contents.OfType<FunctionCallContent>());
                }

                if (toolCalls.Count > 0)
                {
                    events.Add(new AgentStreamEvent("status", "Searching for resources..."));

                    var assistantContents = new List<AIContent>();
                    if (textBuffer.Length > 0)
                        assistantContents.Add(new TextContent(textBuffer.ToString()));
                    assistantContents.AddRange(toolCalls);
                    messages.Add(new ChatMessage(ChatRole.Assistant, assistantContents));

                    // Persist assistant tool-call message to DB
                    _conversationRepository.AddMessage(new ConversationMessage
                    {
                        ConversationId = conversationId,
                        Role = MessageRole.Assistant,
                        Content = textBuffer.Length > 0 ? textBuffer.ToString() : "[tool_calls]",
                        CreatedAt = DateTime.UtcNow
                    });

                    foreach (var tc in toolCalls)
                    {
                        var result = await searchTool.InvokeAsync(tc.Arguments, ct);
                        messages.Add(new ChatMessage(ChatRole.Tool,
                            [new FunctionResultContent(tc.CallId, tc.Name, result)]));

                        // Persist tool result message to DB
                        _conversationRepository.AddMessage(new ConversationMessage
                        {
                            ConversationId = conversationId,
                            Role = MessageRole.Tool,
                            Content = result?.ToString() ?? string.Empty,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    await _unitOfWork.SaveChangesAsync(ct);
                    continue;
                }

                var fullText = textBuffer.ToString();
                var resultEvent = await ProcessCompletedResponseAsync(conversation, fullText, ct);

                if (resultEvent is not null)
                    events.Add(resultEvent);

                _conversationRepository.AddMessage(new ConversationMessage
                {
                    ConversationId = conversationId,
                    Role = MessageRole.Assistant,
                    Content = fullText,
                    CreatedAt = DateTime.UtcNow
                });
                await _unitOfWork.SaveChangesAsync(ct);

                events.Add(new AgentStreamEvent("done", null));
                return new AgentLoopResult(events, null, false);
            }

            // Exhausted all iterations
            return new AgentLoopResult(events, "Max iterations reached", false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation("Stream cancelled for conversation {ConversationId}", conversationId);
            return new AgentLoopResult(events, null, Cancelled: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent loop failed for conversation {ConversationId}", conversationId);
            return new AgentLoopResult(events, ex.Message, false);
        }
    }

    private sealed record AgentLoopResult(
        List<AgentStreamEvent> Events,
        string? Error,
        bool Cancelled);

    private async Task<AgentStreamEvent?> ProcessCompletedResponseAsync(
        AgentConversation conversation,
        string fullText,
        CancellationToken ct)
    {
        var json = ExtractJsonFromMarkers(fullText);
        if (json is null) return null;

        if (conversation.RoadmapId is null)
        {
            var roadmapData = JsonSerializer.Deserialize<RoadmapJson>(json, JsonOptions)!;
            var roadmap = CreateRoadmapEntity(roadmapData, conversation);
            _roadmapRepository.Add(roadmap);
            conversation.RoadmapId = roadmap.Id;
            conversation.Status = ConversationStatus.Completed;
            await _unitOfWork.SaveChangesAsync(ct);
            return new AgentStreamEvent("roadmap_complete", new { roadmapId = roadmap.Id });
        }
        else
        {
            var phaseData = JsonSerializer.Deserialize<PhaseJson>(json, JsonOptions)!;
            if (phaseData.PhaseId is null)
                throw new InvalidOperationException("Phase edit JSON missing phaseId");

            var phase = await _roadmapRepository.GetPhaseByIdAsync(phaseData.PhaseId.Value, ct)
                ?? throw new NotFoundException(nameof(RoadmapPhase), phaseData.PhaseId.Value);

            UpdatePhaseEntity(phase, phaseData);
            await _unitOfWork.SaveChangesAsync(ct);
            return new AgentStreamEvent("phase_updated", new { phaseId = phase.Id });
        }
    }

    private static ChatMessage ToChatMessage(ConversationMessage msg) => new(
        msg.Role switch
        {
            MessageRole.System => ChatRole.System,
            MessageRole.User => ChatRole.User,
            MessageRole.Assistant => ChatRole.Assistant,
            MessageRole.Tool => ChatRole.Tool,
            _ => throw new ArgumentOutOfRangeException(nameof(msg))
        },
        msg.Content);

    private static string? ExtractJsonFromMarkers(string text)
    {
        const string startMarker = "StartOfAnswer";
        const string endMarker = "EndOfAnswer";

        var startIdx = text.IndexOf(startMarker, StringComparison.Ordinal);
        if (startIdx < 0) return null;

        startIdx += startMarker.Length;
        var endIdx = text.IndexOf(endMarker, startIdx, StringComparison.Ordinal);
        if (endIdx < 0) return null;

        return text[startIdx..endIdx].Trim();
    }

    private static Roadmap CreateRoadmapEntity(RoadmapJson data, AgentConversation conversation)
    {
        var roadmap = new Roadmap
        {
            UserId = conversation.UserId,
            ConversationId = conversation.Id,
            Title = data.Title,
            Topic = data.Topic,
            Description = data.Description,
            EstimatedDurationWeeks = data.EstimatedDurationWeeks,
            Status = RoadmapStatus.Completed
        };

        foreach (var p in data.Phases)
        {
            roadmap.Phases.Add(new RoadmapPhase
            {
                RoadmapId = roadmap.Id,
                Title = p.Title,
                Description = p.Description,
                Order = p.Order,
                DurationEstimateWeeks = p.DurationEstimateWeeks,
                Resources = p.Resources,
                Projects = p.Projects,
                Insights = p.Insights
            });
        }

        return roadmap;
    }

    private static void UpdatePhaseEntity(RoadmapPhase phase, PhaseJson data)
    {
        phase.Title = data.Title;
        phase.Description = data.Description;
        phase.Order = data.Order;
        phase.DurationEstimateWeeks = data.DurationEstimateWeeks;
        phase.Resources = data.Resources;
        phase.Projects = data.Projects;
        phase.Insights = data.Insights;
    }

    #region JSON DTOs

    private sealed class RoadmapJson
    {
        public string Title { get; set; } = "";
        public string Topic { get; set; } = "";
        public string Description { get; set; } = "";
        public int EstimatedDurationWeeks { get; set; }
        public List<PhaseJson> Phases { get; set; } = [];
    }

    private sealed class PhaseJson
    {
        public Guid? PhaseId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int Order { get; set; }
        public int DurationEstimateWeeks { get; set; }
        public List<PhaseResource> Resources { get; set; } = [];
        public List<PhaseProject> Projects { get; set; } = [];
        public List<string> Insights { get; set; } = [];
    }

    #endregion
}
