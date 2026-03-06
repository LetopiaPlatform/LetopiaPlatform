using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.API.Core.Reaction;
using LetopiaPlatform.Core.DTOs.Reaction;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Interfaces;
/// <summary>
/// Defines operations for managing reactions on posts and comments.
/// Supports toggling reactions, calculating counters, and retrieving current state.
/// </summary>
public interface IReactionService
{
    /// <summary>
    /// Adds, removes, or switches a reaction for a given target entity.
    /// </summary>
    /// <param name="targetType">The type of target entity. Must be "Post" or "Comment".</param>
    /// <param name="targetId">The unique identifier of the target entity.</param>
    /// <param name="request">The reaction request containing the desired reaction type.</param>
    /// <param name="userId">The ID of the user performing the reaction.</param>
    /// <param name="ct">Optional cancellation token for async operations.</param>
    /// <returns>
    /// A <see cref="ReactionResultDto"/> containing the user's current reaction,
    /// total upvotes, and total downvotes for the target entity.
    /// </returns>
    Task<ReactionResultDto> ToggleAsync(
        TargetType targetType,
        Guid targetId,
        ToggleReactionRequest request,
        Guid userId,
        CancellationToken ct = default);
}
