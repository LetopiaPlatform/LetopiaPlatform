using LetopiaPlatform.Agent.Models;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace LetopiaPlatform.Agent;

public static class ChatClientExtensions
{
    public static AIAgent CreateAIAgent(this ChatClient client, string systemPrompt, IEnumerable<ChatTool> tools, ILogger? logger = null)
    {
        return new AIAgent(client, systemPrompt, tools, logger);
    }
}
