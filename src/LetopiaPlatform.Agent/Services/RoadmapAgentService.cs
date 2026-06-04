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
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        return options;
    }

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

    public async Task ValidateConversationOwnershipAsync(Guid conversationId, Guid userId, CancellationToken ct)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, ct)
            ?? throw new NotFoundException(nameof(AgentConversation), conversationId);

        if (conversation.UserId != userId)
            throw new ForbiddenException();
    }

    public async Task<List<ConversationSummaryDto>> GetUserConversationsAsync(Guid userId, CancellationToken ct)
    {
        var conversations = await _conversationRepository.GetByUserIdAsync(userId, ct);

        return conversations
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => new ConversationSummaryDto(
                c.Id,
                c.Title,
                c.AgentType,
                c.Status,
                c.CreatedAt,
                c.UpdatedAt))
            .ToList();
    }

    public async Task<ConversationDto> GetConversationAsync(Guid conversationId, Guid userId, CancellationToken ct)
    {
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, ct)
            ?? throw new NotFoundException(nameof(AgentConversation), conversationId);

        if (conversation.UserId != userId)
            throw new ForbiddenException();

        return new ConversationDto(
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
    }

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
        bool saveUserMessage = true,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var conversation = await _conversationRepository.GetByIdTrackedAsync(conversationId, ct)
            ?? throw new NotFoundException(nameof(AgentConversation), conversationId);

        if (conversation.UserId != userId)
            throw new ForbiddenException();

        if (saveUserMessage)
        {
            // Persist user message to DB first
            _conversationRepository.AddMessage(new ConversationMessage
            {
                ConversationId = conversationId,
                Role = MessageRole.User,
                Content = userMessage,
                CreatedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync(ct);
        }

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

        // Enforce MaxConversationTokens: trim oldest non-system messages to stay under the limit.
        // Rough estimate: 1 token ≈ 4 characters. Always keep the system prompt + latest user message.
        if (_settings.MaxConversationTokens > 0)
        {
            messages = TrimMessagesToTokenLimit(messages, _settings.MaxConversationTokens);
        }

        var searchTool = WebSearchTool.Create(_webSearchService);
        var options = new ChatOptions
        {
            Tools = [searchTool],
            MaxOutputTokens = _settings.MaxOutputTokens
        };

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
            for (int i = 0; i < _settings.MaxIterations; i++)
            {
                // Re-apply trimming before each LLM call — tool results may have inflated the history
                if (_settings.MaxConversationTokens > 0)
                {
                    messages = TrimMessagesToTokenLimit(messages, _settings.MaxConversationTokens);
                }

                if (i >= 4)
                {
                    options.Tools = null;
                }

                var textBuffer = new StringBuilder();
                var toolCalls = new List<FunctionCallContent>();

                _logger.LogInformation(
                    "LLM call #{Iteration}: {MessageCount} messages, ~{EstTokens} estimated tokens",
                    i + 1, messages.Count, messages.Sum(EstimateTokens));

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

                    var serializedCalls = new SerializedAssistantToolCalls
                    {
                        Text = textBuffer.Length > 0 ? textBuffer.ToString() : null,
                        Calls = toolCalls.Select(tc => new SerializedToolCall
                        {
                            CallId = tc.CallId,
                            Name = tc.Name,
                            Arguments = tc.Arguments
                        }).ToList()
                    };
                    string contentToSave = "@@TOOL_CALLS@@" + JsonSerializer.Serialize(serializedCalls, JsonOptions);

                    // Persist assistant tool-call message to DB
                    _conversationRepository.AddMessage(new ConversationMessage
                    {
                        ConversationId = conversationId,
                        Role = MessageRole.Assistant,
                        Content = contentToSave,
                        CreatedAt = DateTime.UtcNow
                    });

                    foreach (var tc in toolCalls)
                    {
                        var result = await searchTool.InvokeAsync(tc.Arguments, ct);
                        messages.Add(new ChatMessage(ChatRole.Tool,
                            [new FunctionResultContent(tc.CallId, tc.Name, result)]));

                        var serializedResult = new SerializedToolResult
                        {
                            CallId = tc.CallId,
                            Name = tc.Name,
                            Result = result
                        };
                        string resultToSave = "@@TOOL_RESULT@@" + JsonSerializer.Serialize(serializedResult, JsonOptions);

                        // Persist tool result message to DB
                        _conversationRepository.AddMessage(new ConversationMessage
                        {
                            ConversationId = conversationId,
                            Role = MessageRole.Tool,
                            Content = resultToSave,
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
        if (json is null)
        {
            _logger.LogInformation("LLM produced conversational response (no JSON found).");
            return null;
        }

        json = NormalizeLlmOutput(json);
        json = RepairJsonBrackets(json);

        if (conversation.RoadmapId is null)
        {
            try
            {
                if (JsonSerializer.Deserialize<RoadmapJson>(json, JsonOptions) is not { } roadmapData)
                    return new AgentStreamEvent("error", "Failed to parse roadmap data.");

                roadmapData.EstimatedDurationWeeks = roadmapData.Phases.Sum(p => p.DurationEstimateWeeks);

                var roadmap = CreateRoadmapEntity(roadmapData, conversation);
                _roadmapRepository.Add(roadmap);
                conversation.RoadmapId = roadmap.Id;
                conversation.Status = ConversationStatus.Completed;
                return new AgentStreamEvent("roadmap_complete", new { roadmapId = roadmap.Id });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning("LLM produced malformed roadmap JSON for conversation {ConversationId}. Error: {Error}. JSON: {Json}",
                    conversation.Id, ex.Message, json);
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
                _logger.LogWarning("LLM produced malformed phase JSON for conversation {ConversationId}. Error: {Error}. JSON: {Json}",
                    conversation.Id, ex.Message, json);
                return new AgentStreamEvent("error", "Failed to parse phase data.");
            }
        }
    }

    private static ChatMessage ToChatMessage(ConversationMessage msg)
    {
        if (msg.Role == MessageRole.Assistant && msg.Content.StartsWith("@@TOOL_CALLS@@", StringComparison.Ordinal))
        {
            var json = msg.Content["@@TOOL_CALLS@@".Length..];
            var data = JsonSerializer.Deserialize<SerializedAssistantToolCalls>(json, JsonOptions);
            var contents = new List<AIContent>();
            if (!string.IsNullOrEmpty(data?.Text))
                contents.Add(new TextContent(data.Text));
            
            if (data?.Calls != null)
            {
                foreach (var call in data.Calls)
                {
                    contents.Add(new FunctionCallContent(call.CallId, call.Name, call.Arguments));
                }
            }
            return new ChatMessage(ChatRole.Assistant, contents);
        }

        if (msg.Role == MessageRole.Tool && msg.Content.StartsWith("@@TOOL_RESULT@@", StringComparison.Ordinal))
        {
            var json = msg.Content["@@TOOL_RESULT@@".Length..];
            var data = JsonSerializer.Deserialize<SerializedToolResult>(json, JsonOptions);
            if (data != null)
            {
                return new ChatMessage(ChatRole.Tool, [new FunctionResultContent(data.CallId, data.Name, data.Result)]);
            }
        }

        if (msg.Role == MessageRole.Tool)
        {
            return new ChatMessage(ChatRole.Tool, [new FunctionResultContent("unknown", "unknown", msg.Content)]);
        }

        return new ChatMessage(
            msg.Role switch
            {
                MessageRole.System => ChatRole.System,
                MessageRole.User => ChatRole.User,
                MessageRole.Assistant => ChatRole.Assistant,
                _ => throw new ArgumentOutOfRangeException(nameof(msg))
            },
            msg.Content);
    }

    /// <summary>
    /// Estimates the token count of a chat message using a rough 1 token ≈ 4 characters heuristic.
    /// Accounts for text content, function call arguments, and function result payloads.
    /// </summary>
    private static int EstimateTokens(ChatMessage m)
    {
        const int charsPerToken = 4;
        int chars = m.Text?.Length ?? 0;
        foreach (var content in m.Contents)
        {
            if (content is FunctionResultContent frc)
                chars += frc.Result?.ToString()?.Length ?? 0;
            else if (content is FunctionCallContent fcc)
                chars += fcc.Arguments?.ToString()?.Length ?? 0;
        }
        return chars / charsPerToken;
    }

    /// <summary>
    /// Trims the message list to stay within the approximate token limit.
    /// Always keeps the system prompt (index 0) and the current turn
    /// (last user message + all subsequent tool-call/result pairs).
    /// Drops the oldest non-system messages first.
    /// </summary>
    private static List<ChatMessage> TrimMessagesToTokenLimit(List<ChatMessage> messages, int maxTokens)
    {
        int totalTokens = messages.Sum(EstimateTokens);
        if (totalTokens <= maxTokens)
            return messages;

        // Find the last user message — protect it and everything after it
        // (tool calls, tool results) to avoid orphaned pairs.
        int lastUserIdx = messages.Count - 1;
        for (int i = messages.Count - 1; i >= 1; i--)
        {
            if (messages[i].Role == ChatRole.User)
            {
                lastUserIdx = i;
                break;
            }
        }

        // Only trim between index 1 and lastUserIdx (exclusive)
        var trimmed = new List<ChatMessage>(messages);
        while (trimmed.Count > 2 && totalTokens > maxTokens && lastUserIdx > 1)
        {
            totalTokens -= EstimateTokens(trimmed[1]);
            trimmed.RemoveAt(1);
            lastUserIdx--;
        }

        return trimmed;
    }

    private static string? ExtractJsonFromMarkers(string text)
    {
        const string startMarker = "StartOfAnswer";
        const string endMarker = "EndOfAnswer";

        var startIdx = text.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);

        string possibleJson;
        if (startIdx >= 0)
        {
            startIdx += startMarker.Length;
            var endIdx = text.IndexOf(endMarker, startIdx, StringComparison.OrdinalIgnoreCase);
            possibleJson = endIdx > startIdx
                ? text[startIdx..endIdx].Trim()
                : text[startIdx..].Trim();
        }
        else
        {
            possibleJson = text.Trim();
        }

        // Clean up markdown code blocks if LLM added them
        if (possibleJson.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            possibleJson = possibleJson[7..].TrimStart();
        else if (possibleJson.StartsWith("```", StringComparison.OrdinalIgnoreCase))
            possibleJson = possibleJson[3..].TrimStart();

        if (possibleJson.EndsWith("```", StringComparison.OrdinalIgnoreCase))
            possibleJson = possibleJson[..^3].TrimEnd();

        // Fallback: string-aware bracket-depth scan.
        // Ignores { } characters inside string literals to avoid false depth counts.
        // Correctly stops at the real root closing brace, ignoring any garbage after it.
        var firstBrace = possibleJson.IndexOf('{');
        if (firstBrace >= 0)
        {
            var depth = 0;
            var inString = false;
            var escape = false;

            for (var i = firstBrace; i < possibleJson.Length; i++)
            {
                var c = possibleJson[i];

                if (escape) { escape = false; continue; }
                if (c == '\\' && inString) { escape = true; continue; }
                if (c == '"') { inString = !inString; continue; }
                if (inString) continue;

                if (c == '{') depth++;
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0)
                        return possibleJson[firstBrace..(i + 1)];
                }
            }
        }

        return null;
    }

    private static string NormalizeLlmOutput(string json)
    {
        return json
            .Replace("\"Guide\"", "\"Article\"")
            .Replace("\"Tutorial\"", "\"Course\"")
            .Replace("\"Workshop\"", "\"Course\"")
            .Replace("\"Reference\"", "\"Documentation\"");
    }

    /// <summary>
    /// Best-effort repair of malformed JSON brackets from LLM output.
    /// Handles: extra closing brackets, mismatched bracket types,
    /// and truncated responses missing closing brackets.
    /// </summary>
    private static string RepairJsonBrackets(string json)
    {
        var stack = new Stack<char>();
        var sb = new StringBuilder();
        var inString = false;
        var escape = false;

        foreach (var c in json)
        {
            if (escape) { escape = false; sb.Append(c); continue; }
            if (c == '\\' && inString) { escape = true; sb.Append(c); continue; }
            if (c == '"') { inString = !inString; sb.Append(c); continue; }
            if (inString) { sb.Append(c); continue; }

            switch (c)
            {
                case '{':
                    stack.Push('{');
                    sb.Append(c);
                    break;
                case '[':
                    stack.Push('[');
                    sb.Append(c);
                    break;
                case '}':
                    if (stack.Count > 0 && stack.Peek() == '{')
                    {
                        stack.Pop();
                        sb.Append(c);
                    }
                    else if (stack.Count > 0 && stack.Peek() == '[')
                    {
                        // Mismatched: expected ] but got } — close the array first
                        stack.Pop();
                        sb.Append(']');
                        // Now handle the } if there's a matching {
                        if (stack.Count > 0 && stack.Peek() == '{')
                        {
                            stack.Pop();
                            sb.Append(c);
                        }
                    }
                    // else: extra } with nothing on stack — drop it (garbage token)
                    break;
                case ']':
                    if (stack.Count > 0 && stack.Peek() == '[')
                    {
                        stack.Pop();
                        sb.Append(c);
                    }
                    // else: extra ] with nothing matching — drop it
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        // Append any missing closing brackets for truncated responses
        while (stack.Count > 0)
        {
            sb.Append(stack.Pop() == '{' ? '}' : ']');
        }

        return sb.ToString();
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

    private sealed class SerializedAssistantToolCalls
    {
        public string? Text { get; set; }
        public List<SerializedToolCall> Calls { get; set; } = [];
    }

    private sealed class SerializedToolCall
    {
        public string CallId { get; set; } = "";
        public string Name { get; set; } = "";
        public IDictionary<string, object?>? Arguments { get; set; }
    }

    private sealed class SerializedToolResult
    {
        public string CallId { get; set; } = "";
        public string Name { get; set; } = "";
        public object? Result { get; set; }
    }

    #endregion
}
