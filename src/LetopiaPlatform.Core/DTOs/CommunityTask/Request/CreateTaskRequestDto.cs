namespace LetopiaPlatform.Core.DTOs.CommunityTask.Request;
public record CreateTaskRequestDto(
    string Title,
    string? Description,
    DateTime Deadline,
    Guid? CategoryId
);
