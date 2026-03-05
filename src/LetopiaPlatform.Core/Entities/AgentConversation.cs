using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Entities;

/// <summary>
/// Represents a conversation session between a user and an agent.
/// </summary>
public class AgentConversation : AuditableEntity
{
    /// <summary>
    /// Foreign key to the User who initiated the conversation.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Nullable foreign key to the generated Roadmap.
    /// Set after the agent completes roadmap generation.
    /// </summary>
    public Guid? RoadmapId { get; set; }

    /// <summary>
    /// The type of agent handling this conversation (e.g., "RoadmapGenerator").
    /// </summary>
    public required string AgentType { get; set; }

    /// <summary>
    /// The current status of the conversation.
    /// </summary>
    public ConversationStatus Status { get; set; } = ConversationStatus.Active;

    /// <summary>
    /// The title or subject of the conversation.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Navigation property to the User who initiated this conversation.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Navigation property to the generated Roadmap (nullable until generation completes).
    /// </summary>
    public Roadmap? Roadmap { get; set; }

    /// <summary>
    /// Collection of messages exchanged in this conversation.
    /// </summary>
    public ICollection<ConversationMessage> Messages { get; set; } = [];
}
