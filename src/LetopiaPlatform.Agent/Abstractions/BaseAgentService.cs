using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using LetopiaPlatform.Agent; // For ChatClientExtensions
using LetopiaPlatform.Agent.Configuration;
using LetopiaPlatform.Agent.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace LetopiaPlatform.Agent.Abstractions;

public abstract class BaseAgentService : IAgentService
{
    protected readonly AgentSettings _settings;
    protected readonly ILogger _logger;
    protected readonly IConversationCache _conversationCache;

    protected readonly OpenAIClient _openAIClient;
    protected readonly ChatClient _chatClient;
    protected readonly AIAgent _agent;

    public abstract string AgentType { get; }

    protected BaseAgentService(
        IOptions<AgentSettings> options,
        ILogger logger,
        IConversationCache conversationCache)
    {
        _settings = options.Value;
        _logger = logger;
        _conversationCache = conversationCache;

        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(_settings.GitHubModelsEndpoint)
        };

        _openAIClient = new OpenAIClient(
            new ApiKeyCredential(_settings.GitHubToken),
            clientOptions);

        _chatClient = _openAIClient.GetChatClient(_settings.ModelId);
        _agent = _chatClient.CreateAIAgent(GetSystemPrompt(), GetTools(), logger: _logger);

        _logger.LogInformation(
            "Initializing Agent {AgentType} with Model {ModelId}",
            AgentType,
            _settings.ModelId);
    }

    protected abstract string GetSystemPrompt();
    protected abstract IEnumerable<ChatTool> GetTools();

    public virtual string CreateNewThread()
        => $"{AgentType}:{Guid.NewGuid()}";

    protected virtual async Task<ConversationThread> GetOrCreateThreadAsync(string? threadId)
    {
        if (!string.IsNullOrWhiteSpace(threadId))
        {
            var cached = await _conversationCache.GetAsync(threadId);
            if (cached != null)
                return cached;
        }

        return new ConversationThread
        {
            ThreadId = CreateNewThread()
        };
    }

    protected virtual Task SaveThreadAsync(string threadId, ConversationThread thread)
        => _conversationCache.SetAsync(threadId, thread);

    private static readonly AsyncPolicy _retryPolicy =
        Policy
            .Handle<ClientResultException>(ex => ex.Status == 429 || ex.Status >= 500)
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)));

    public virtual async Task<string> RunAsync(
        string userInput,
        string? threadId,
        CancellationToken cancellationToken = default)
    {
        var thread = await GetOrCreateThreadAsync(threadId);
        var correlationId = Guid.NewGuid().ToString();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["ThreadId"] = thread.ThreadId,
            ["AgentType"] = AgentType
        });

        _logger.LogInformation("RunAsync started for Thread {ThreadId}", thread.ThreadId);

        thread.Messages.Add(new UserChatMessage(userInput));

        string response = await _retryPolicy.ExecuteAsync(async () =>
        {
            // Pass the full conversation history to the agent
            return await _agent.RunAsync(thread.Messages, cancellationToken);
        });

        thread.Messages.Add(new AssistantChatMessage(response));
        await SaveThreadAsync(thread.ThreadId, thread);

        _logger.LogInformation("RunAsync completed for Thread {ThreadId}", thread.ThreadId);

        return response;
    }

    public virtual async IAsyncEnumerable<string> RunStreamingAsync(
        string userInput,
        string? threadId,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var thread = await GetOrCreateThreadAsync(threadId);
        var correlationId = Guid.NewGuid().ToString();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["ThreadId"] = thread.ThreadId,
            ["AgentType"] = AgentType
        });

        _logger.LogInformation("RunStreamingAsync started for Thread {ThreadId}", thread.ThreadId);

        thread.Messages.Add(new UserChatMessage(userInput));
        var assistantText = "";

        await foreach (var chunk in _agent.RunStreamingAsync(thread.Messages, cancellationToken))
        {
            assistantText += chunk;
            yield return chunk;
        }

        thread.Messages.Add(new AssistantChatMessage(assistantText));
        await SaveThreadAsync(thread.ThreadId, thread);

        _logger.LogInformation("RunStreamingAsync completed for Thread {ThreadId}", thread.ThreadId);
    }
}
