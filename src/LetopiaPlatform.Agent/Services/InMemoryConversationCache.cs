using System.Collections.Concurrent;
using LetopiaPlatform.Agent.Abstractions;
using LetopiaPlatform.Agent.Models;

namespace LetopiaPlatform.Agent.Services;

public class InMemoryConversationCache : IConversationCache
{
    private readonly ConcurrentDictionary<string, ConversationThread> _cache = new();

    public Task<ConversationThread?> GetAsync(string threadId)
    {
        _cache.TryGetValue(threadId, out var thread);
        return Task.FromResult(thread);
    }

    public Task SetAsync(string threadId, ConversationThread thread)
    {
        _cache[threadId] = thread;
        return Task.CompletedTask;
    }
}
