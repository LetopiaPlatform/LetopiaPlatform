using LetopiaPlatform.Core.Entities;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Data access operations for agent conversations and messages.
/// Does NOT manage persistence — use <see cref="IUnitOfWork{TContext}"/> for SaveChanges and transactions.
/// </summary>
public interface IConversationRepository
{
    /// <summary>
    /// Retrieves a conversation by its unique identifier, including all messages ordered by creation date.
    /// </summary>
    /// <param name="id">The conversation ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The conversation with messages if found; otherwise null.</returns>
    Task<AgentConversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a conversation by its unique identifier without loading messages.
    /// </summary>
    /// <param name="id">The conversation ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The conversation if found; otherwise null.</returns>
    Task<AgentConversation?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a conversation by its unique identifier with tracking enabled, allowing property updates.
    /// </summary>
    /// <param name="id">The conversation ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked conversation if found; otherwise null.</returns>
    Task<AgentConversation?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all conversations belonging to a user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of conversations for the user.</returns>
    Task<List<AgentConversation>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new conversation to the repository.
    /// </summary>
    /// <param name="conversation">The conversation to add.</param>
    void Add(AgentConversation conversation);

    /// <summary>
    /// Adds a new message to the repository.
    /// </summary>
    /// <param name="message">The message to add.</param>
    void AddMessage(ConversationMessage message);
}