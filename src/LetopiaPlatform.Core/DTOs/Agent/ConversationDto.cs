using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Agent;

/// <summary>
/// Full conversation representation including all messages.
/// </summary>
public sealed record ConversationDto(
    Guid Id,
    string Title,
    AgentType AgentType,
    ConversationStatus Status,
    Guid? RoadmapId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ConversationMessageDto> Messages);

/// <summary>
/// A single message within a conversation.
/// </summary>
public sealed record ConversationMessageDto(
    Guid Id,
    MessageRole Role,
    string Content,
    DateTime CreatedAt);
