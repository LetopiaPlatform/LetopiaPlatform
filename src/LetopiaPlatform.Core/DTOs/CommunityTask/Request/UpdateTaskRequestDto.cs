namespace LetopiaPlatform.Core.DTOs.CommunityTask.Request;
public record UpdateTaskRequestDto(
    string Title,
    string? Description,
    DateTime Deadline,
    Guid? CategoryId
);
