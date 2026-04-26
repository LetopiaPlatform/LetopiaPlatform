using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using LetopiaPlatform.Agent.Configuration;
using LetopiaPlatform.Agent.Prompts;
using LetopiaPlatform.Agent.Tools;
using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Exceptions;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LetopiaPlatform.Agent.Services;

public class RoadmapAgentService : IRoadmapAgentService
{
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
    private readonly AgentSettings _settings;

    public RoadmapAgentService(
        IChatClient chatClient,
        IWebSearchService webSearchService,
        IRoadmapRepository roadmapRepository,
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        ILogger<RoadmapAgentService> logger,
        IOptions<AgentSettings> settings)
    {
        _chatClient = chatClient;
        _webSearchService = webSearchService;
        _roadmapRepository = roadmapRepository;
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _settings = settings.Value;
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

        // Persist user message to DB first
        _conversationRepository.AddMessage(new ConversationMessage
        {
            ConversationId = conversationId,
            Role = MessageRole.User,
            Content = userMessage,
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync(ct);

        // Build the chat history from the persisted messages which now include
        // the user message we just saved — no need to add it again manually.
        var messages = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(ToChatMessage)
            .ToList();

        // Guard: if the last persisted message is not the user message we just added
        // (e.g. EF tracking didn't append it to the navigation), add it to avoid a missing turn.
        if (messages.Count == 0 || messages[^1].Role != ChatRole.User
            || messages[^1].Text != userMessage)
        {
            messages.Add(new ChatMessage(ChatRole.User, userMessage));
        }

        var searchTool = WebSearchTool.Create(_webSearchService);
        var options = new ChatOptions { Tools = [searchTool] };

        // Producer-consumer: the agent loop writes events to a Channel,
        // and we yield them here in real-time as they arrive.
        var channel = System.Threading.Channels.Channel.CreateUnbounded<AgentStreamEvent>(
            new System.Threading.Channels.UnboundedChannelOptions { SingleWriter = true });

        // Fire the producer as a background task
        _ = RunAgentLoopAsync(conversation, conversationId, messages,
            searchTool, options, channel.Writer, ct);

        // Consumer: yield each event as soon as it arrives
        await foreach (AgentStreamEvent ev in channel.Reader.ReadAllAsync(ct))
        {
            yield return ev;
        }
    }

    /// <summary>
    /// Runs the agent while-loop, writing events to the channel writer in real-time.
    /// Guarantees the writer is completed when the method exits (success or failure).
    /// </summary>
    private async Task RunAgentLoopAsync(
        AgentConversation conversation,
        Guid conversationId,
        List<ChatMessage> messages,
        AIFunction searchTool,
        ChatOptions options,
        ChannelWriter<AgentStreamEvent> writer,
        CancellationToken ct)
    {
        try
        {
            for (int i = 0; i < _settings.MaxAgentIterations; i++)
            {
                var textBuffer = new StringBuilder();
                var toolCalls = new List<FunctionCallContent>();

                await foreach (var update in _chatClient.CompleteStreamingAsync(messages, options, ct))
                {
                    if (update.Text is not null)
                    {
                        textBuffer.Append(update.Text);
                        await writer.WriteAsync(new AgentStreamEvent("delta", update.Text), ct);
                    }

                    toolCalls.AddRange(update.Contents.OfType<FunctionCallContent>());
                }

                if (toolCalls.Count > 0)
                {
                    await writer.WriteAsync(
                        new AgentStreamEvent("status", "Searching for resources..."), ct);

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
                    await writer.WriteAsync(resultEvent, ct);

                _conversationRepository.AddMessage(new ConversationMessage
                {
                    ConversationId = conversationId,
                    Role = MessageRole.Assistant,
                    Content = fullText,
                    CreatedAt = DateTime.UtcNow
                });
                await _unitOfWork.SaveChangesAsync(ct);

                await writer.WriteAsync(new AgentStreamEvent("done", null), ct);
                return;
            }

            // Exhausted all iterations
            await writer.WriteAsync(
                new AgentStreamEvent("error", "Max iterations reached"), ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Stream cancelled for conversation {ConversationId}", conversationId);
            // Channel completes in finally — consumer stops reading
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Agent loop failed for conversation {ConversationId}", conversationId);
            // Never leak ex.Message to the client — log it above, return a safe message
            try
            {
                await writer.WriteAsync(
                    new AgentStreamEvent("error", "An unexpected error occurred. Please try again."), ct);
            }
            catch
            {
                // Consumer may have already disconnected; swallow
            }
        }
        finally
        {
            writer.Complete();
        }
    }

    /// <summary>
    /// Inspects the completed LLM response for structured JSON between markers.
    /// If found, persists the roadmap/phase and returns the corresponding event.
    /// Returns null if no structured data markers are present.
    /// </summary>
    private async Task<AgentStreamEvent?> ProcessCompletedResponseAsync(
        AgentConversation conversation,
        string fullText,
        CancellationToken ct)
    {
        var json = ExtractJsonFromMarkers(fullText);
        if (json is null) return null;

        if (conversation.RoadmapId is null)
        {
            try
            {
                if (JsonSerializer.Deserialize<RoadmapJson>(json, JsonOptions) is not { } roadmapData)
                    return new AgentStreamEvent("error", "Failed to parse roadmap data.");

                var roadmap = CreateRoadmapEntity(roadmapData, conversation);
                _roadmapRepository.Add(roadmap);
                conversation.RoadmapId = roadmap.Id;
                conversation.Status = ConversationStatus.Completed;
                return new AgentStreamEvent("roadmap_complete", new { roadmapId = roadmap.Id });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex,
                    "LLM produced malformed roadmap JSON for conversation {ConversationId}",
                    conversation.Id);
                return new AgentStreamEvent("error", "Failed to parse roadmap data.");
            }
        }
        else
        {
            try
            {
                if (JsonSerializer.Deserialize<PhaseJson>(json, JsonOptions) is not { } phaseData)
                    return new AgentStreamEvent("error", "Failed to parse phase data.");

                if (phaseData.PhaseId is null)
                    return new AgentStreamEvent("error", "Phase update is missing a phase identifier.");

                var phase = await _roadmapRepository.GetPhaseByIdAsync(phaseData.PhaseId.Value, ct)
                    ?? throw new NotFoundException(nameof(RoadmapPhase), phaseData.PhaseId.Value);

                UpdatePhaseEntity(phase, phaseData);
                return new AgentStreamEvent("phase_updated", new { phaseId = phase.Id });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex,
                    "LLM produced malformed phase JSON for conversation {ConversationId}",
                    conversation.Id);
                return new AgentStreamEvent("error", "Failed to parse phase data.");
            }
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
