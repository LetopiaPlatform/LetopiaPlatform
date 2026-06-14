using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Defines the contract for an AI agent that can hold conversations and stream responses.
/// Each implementation serves a specific <see cref="AgentType"/>.
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Gets the agent type identifier handled by this service (e.g., "RoadmapGenerator").
    /// </summary>
    AgentType AgentType { get; }

    /// <summary>
    /// Starts a new conversation session for the specified user.
    /// </summary>
    Task<AgentConversation> StartConversationAsync(
        Guid userId,
        string initialMessage,
        CancellationToken ct);

    /// <summary>
    /// Processes a user message and streams back agent events (tokens, status updates, etc.).
    /// </summary>
    IAsyncEnumerable<AgentStreamEvent> ProcessMessageAsync(
        Guid conversationId,
        string userMessage,
        Guid userId,
        bool saveUserMessage = true,
        CancellationToken ct = default);
}
