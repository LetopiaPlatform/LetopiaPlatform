using LetopiaPlatform.Core.DTOs.Agent;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Service contract for the roadmap agent.
/// Extends <see cref="IAgentService"/> with conversation query and ownership operations.
/// </summary>
public interface IRoadmapAgentService : IAgentService
{
    /// <summary>
    /// Validates that the specified conversation exists and belongs to the given user.
    /// </summary>
    /// <exception cref="Core.Exceptions.NotFoundException">Thrown when the conversation does not exist.</exception>
    /// <exception cref="Core.Exceptions.ForbiddenException">Thrown when the conversation belongs to another user.</exception>
    Task ValidateConversationOwnershipAsync(Guid conversationId, Guid userId, CancellationToken ct);

    /// <summary>
    /// Retrieves all conversations for the specified user, ordered by most recently updated.
    /// </summary>
    Task<List<ConversationSummaryDto>> GetUserConversationsAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// Retrieves a full conversation (including messages) after verifying ownership.
    /// </summary>
    /// <exception cref="Core.Exceptions.NotFoundException">Thrown when the conversation does not exist.</exception>
    /// <exception cref="Core.Exceptions.ForbiddenException">Thrown when the conversation belongs to another user.</exception>
    Task<ConversationDto> GetConversationAsync(Guid conversationId, Guid userId, CancellationToken ct);
}
