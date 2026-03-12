using System.ComponentModel.DataAnnotations;

namespace LetopiaPlatform.Core.DTOs.Agent;

/// <summary>
/// Request to start a new agent conversation.
/// </summary>
public sealed record StartConversationRequest(
    [Required] string InitialMessage);
