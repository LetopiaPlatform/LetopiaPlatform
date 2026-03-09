using System.ComponentModel.DataAnnotations;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Agent;

/// <summary>
/// Request to update the status of a roadmap phase.
/// </summary>
public sealed record UpdatePhaseStatusRequest(
    [Required] PhaseStatus Status);
