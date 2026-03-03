
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Interfaces.Repositories;
/// <summary>
/// Repository interface for managing <see cref="Reaction"/> entities.
/// Supports polymorphic targets such as Post or Comment.
/// </summary>
public interface IReactionRepository : IGenericRepository<Reaction>
{
    /// <summary>
    /// Gets all reactions for a specific target (Post or Comment).
    /// </summary>
    /// <param name="targetType">The target type, e.g., "Post" or "Comment".</param>
    /// <param name="targetId">The target's unique identifier.</param>
    /// <returns>A collection of reactions for the target.</returns>
    Task<IEnumerable<Reaction>> GetReactionsByTargetAsync(TargetType targetType, Guid targetId);

    /// <summary>
    /// Gets the count of reactions for a specific target, optionally filtered by reaction type.
    /// </summary>
    /// <param name="targetType">The target type, e.g., "Post" or "Comment".</param>
    /// <param name="targetId">The target's unique identifier.</param>
    /// <param name="type">Optional reaction type to filter by.</param>
    /// <returns>The number of reactions.</returns>
    Task<int> GetReactionCountAsync(TargetType targetType, Guid targetId, ReactionType? type = null);

    /// <summary>
    /// Checks if a specific user has reacted to a target.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="targetType">The target type, e.g., "Post" or "Comment".</param>
    /// <param name="targetId">The target's unique identifier.</param>
    /// <returns>The reaction if exists; otherwise null.</returns>
    Task<Reaction?> GetUserReactionAsync(Guid userId, TargetType targetType, Guid targetId);
}
