using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Reaction;

/// <summary>
/// Request DTO for reacting to a post.
/// </summary>
public sealed record ReactRequestDto(ReactionType ReactionType);
