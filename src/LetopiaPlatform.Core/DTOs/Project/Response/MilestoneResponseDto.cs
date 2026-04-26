namespace LetopiaPlatform.Core.DTOs.Project.Response;
public record MilestoneResponseDto(
    string Title,
    string? Description,
    DateTime CreatedAt
);
