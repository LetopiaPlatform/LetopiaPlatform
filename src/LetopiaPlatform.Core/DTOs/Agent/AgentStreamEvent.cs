namespace LetopiaPlatform.Core.DTOs.Agent;

/// <summary>
/// Represents a Server-Sent Event (SSE) emitted during agent streaming.
/// </summary>
/// <remarks>
/// <para>Valid <see cref="Type"/> values:</para>
/// <list type="bullet">
///   <item><c>delta</c> — incremental text token from the agent.</item>
///   <item><c>status</c> — conversation status change.</item>
///   <item><c>roadmap_complete</c> — roadmap generation finished; <see cref="Data"/> contains the roadmap payload.</item>
///   <item><c>phase_updated</c> — a phase status was updated.</item>
///   <item><c>error</c> — an error occurred; <see cref="Data"/> contains error details.</item>
///   <item><c>done</c> — stream is complete; no further events will be sent.</item>
/// </list>
/// </remarks>
public sealed record AgentStreamEvent(
    string Type,
    object? Data);

