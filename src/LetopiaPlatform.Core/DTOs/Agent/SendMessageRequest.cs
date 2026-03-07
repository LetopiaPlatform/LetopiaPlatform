using System.ComponentModel.DataAnnotations;

namespace LetopiaPlatform.Core.DTOs.Agent;

/// <summary>
/// Request to send a message within an existing conversation.
/// </summary>
public sealed record SendMessageRequest(
    [Required] string Content);
