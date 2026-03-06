using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Entities;

/// <summary>
/// Represents a single message in an agent conversation.
/// </summary>
public class ConversationMessage : BaseEntity
{
    /// <summary>
    /// Foreign key to the parent AgentConversation.
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// The role of this message (User, Assistant, System, or Tool).
    /// </summary>
    public MessageRole Role { get; set; }

    /// <summary>
    /// The content of the message.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Optional token count for cost monitoring and usage tracking.
    /// </summary>
    public int? TokenCount { get; set; }

    /// <summary>
    /// The timestamp when this message was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Navigation property to the parent AgentConversation.
    /// </summary>
    public AgentConversation Conversation { get; set; } = null!;
}
