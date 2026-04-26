namespace LetopiaPlatform.Core.DTOs.Project.Response;
public record ProjectDetailsResponseDto(
    Guid Id,
    string Title,
    string Description,
    string CategoryName,
    string TimeLeftText,
    List<string> Skills,
    List<string> ProjectGoals,
    List<string> TimelineEvents,
    DateTime StartDate,
    DateTime Deadline,
    string? CoverImageUrl,
    string Status,
    string OwnerName,
    List<MilestoneResponseDto> Milestones
);
