namespace LetopiaPlatform.API.Core.Reaction;

/// <summary>
/// Reaction result DTO, e.g., after reacting to a post.
/// </summary>
public sealed record ReactionResultDto(
    string? CurrentReaction,
    int Upvotes,
    int Downvotes);
