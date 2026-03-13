namespace LetopiaPlatform.Core.DTOs.CommunityTask.Response;
public record CommunityTaskResponseDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime Deadline,
    string? CategoryName,
    string ColorHex,
    string? IconKey,
    bool IsCompleted
);
