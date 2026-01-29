using LetopiaPlatform.Agent.Models;

namespace LetopiaPlatform.Agent.Abstractions;

public interface IConversationCache
{
    Task<ConversationThread?> GetAsync(string threadId);
    Task SetAsync(string threadId, ConversationThread thread);
}
