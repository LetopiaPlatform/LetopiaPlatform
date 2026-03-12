using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Entities;
<<<<<<< HEAD
=======
using LetopiaPlatform.Core.Enums;
>>>>>>> main

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
    /// <param name="userId">The ID of the user initiating the conversation.</param>
    /// <param name="initialMessage">The first message from the user.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created <see cref="AgentConversation"/>.</returns>
    Task<AgentConversation> StartConversationAsync(Guid userId, string initialMessage, CancellationToken ct);

    /// <summary>
    /// Processes a user message and streams back agent events (tokens, status updates, etc.).
    /// </summary>
    /// <param name="conversationId">The conversation to continue.</param>
    /// <param name="userMessage">The new message from the user.</param>
    /// <param name="userId">The ID of the user sending the message.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An asynchronous stream of <see cref="AgentStreamEvent"/>.</returns>
    IAsyncEnumerable<AgentStreamEvent> ProcessMessageAsync(
        Guid conversationId,
        string userMessage,
        Guid userId,
        CancellationToken ct);
}
