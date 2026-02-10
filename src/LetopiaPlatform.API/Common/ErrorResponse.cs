namespace LetopiaPlatform.API.Common;

/// <summary>
/// Standard error response format returned by exception middleware and controllers.
/// Provides a consistent shape for all error payloads across the API.
/// </summary>
public sealed class ErrorResponse
{
    public int Status { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string> Errors { get; init; } = [];
    public string? Details { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
