
using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Entities;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Data access operations for roadmaps and their phases.
/// Does NOT manage persistence — use <see cref="IUnitOfWork{TContext}"/> for SaveChanges and transactions.
/// </summary>
public interface IRoadmapRepository
{
    /// <summary>
    /// Retrieves a roadmap by its unique identifier, including all child phases.
    /// </summary>
    /// <param name="id">The roadmap ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The roadmap with phases if found; otherwise null.</returns>
    Task<Roadmap?> GetByIdWithPhasesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all roadmaps belonging to a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of roadmaps for the user.</returns>
    Task<List<Roadmap>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves lightweight roadmap summaries for a user using server-side projection.
    /// Avoids loading full phase jsonb data (resources, projects, insights).
    /// </summary>
    Task<List<RoadmapSummaryDto>> GetRoadmapSummariesAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a single roadmap phase by its unique identifier.
    /// </summary>
    /// <param name="phaseId">The phase ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The phase if found; otherwise null.</returns>
    Task<RoadmapPhase?> GetPhaseByIdAsync(Guid phaseId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a tracked phase by its ID and parent roadmap ID.
    /// Includes the parent Roadmap navigation for ownership checks.
    /// </summary>
    /// <param name="phaseId">The phase ID.</param>
    /// <param name="roadmapId">The parent roadmap ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked phase with its Roadmap if found; otherwise null.</returns>
    Task<RoadmapPhase?> GetPhaseByRoadmapAsync(Guid phaseId, Guid roadmapId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new roadmap to the repository.
    /// </summary>
    /// <param name="roadmap">The roadmap to add.</param>
    void Add(Roadmap roadmap);

    /// <summary>
    /// Marks an existing roadmap as modified in the repository.
    /// </summary>
    /// <param name="roadmap">The roadmap to update.</param>
    void Update(Roadmap roadmap);
}