using OpenAI.Chat;

namespace LetopiaPlatform.Agent.Models;

public class ConversationThread
{
    public string ThreadId { get; set; } = string.Empty;
    public List<ChatMessage> Messages { get; set; } = new();
}
