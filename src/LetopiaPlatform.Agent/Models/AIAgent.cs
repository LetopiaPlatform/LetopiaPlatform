using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.ClientModel;
using System.Runtime.CompilerServices;

namespace LetopiaPlatform.Agent.Models;

public class AIAgent
{
    private readonly ChatClient _chatClient;
    private readonly string _systemPrompt;
    private readonly IEnumerable<ChatTool> _tools;
    private readonly ILogger? _logger;

    public AIAgent(ChatClient chatClient, string systemPrompt, IEnumerable<ChatTool> tools, ILogger? logger = null)
    {
        _chatClient = chatClient;
        _systemPrompt = systemPrompt;
        _tools = tools;
        _logger = logger;
    }

    public async Task<string> RunAsync(IEnumerable<ChatMessage> history, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(_systemPrompt)
        };
        messages.AddRange(history);

        var options = new ChatCompletionOptions();
        foreach(var tool in _tools)
        {
             options.Tools.Add(tool);
        }

        _logger?.LogInformation("Executing Agent RunAsync with {MessageCount} messages", messages.Count);

        try 
        {
            // TODO: Implement tool usage loop if needed, for now just simple completion or use auto-tool calling if supported by SDK.
            // OpenAI v2 handles tool calling via options, but strict loop is needed for "Agent" behavior usually.
            // For now, assume single turn or SDK handles it if we loop.
            // But requirement says "RunAsync... returns complete response".
            // We'll do a simple completion for now check if basic text generation works.
            
            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            
            // Log tokens usage if available
            // _logger?.LogInformation("Usage: {Usage}", completion.Usage);

            return completion.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in Agent RunAsync");
            throw;
        }
    }

    public async IAsyncEnumerable<string> RunStreamingAsync(IEnumerable<ChatMessage> history, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(_systemPrompt)
        };
        messages.AddRange(history);

         var options = new ChatCompletionOptions();
        foreach(var tool in _tools)
        {
             options.Tools.Add(tool);
        }

        _logger?.LogInformation("Executing Agent RunStreamingAsync");

        AsyncCollectionResult<StreamingChatCompletionUpdate> chunkCollection = _chatClient.CompleteChatStreamingAsync(messages, options, cancellationToken);

        await foreach (var update in chunkCollection)
        {
            if (update.ContentUpdate.Count > 0)
            {
                foreach(var content in update.ContentUpdate)
                {
                    yield return content.Text;
                }
            }
        }
    }
}
