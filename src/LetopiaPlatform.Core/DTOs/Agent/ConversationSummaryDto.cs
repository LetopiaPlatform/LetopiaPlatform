using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Agent;

/// <summary>
/// Lightweight conversation representation for list views.
/// </summary>
public sealed record ConversationSummaryDto(
    Guid Id,
    string Title,
    AgentType AgentType,
    ConversationStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
