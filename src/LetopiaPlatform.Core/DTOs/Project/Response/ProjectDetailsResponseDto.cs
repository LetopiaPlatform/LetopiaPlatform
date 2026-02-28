namespace LetopiaPlatform.Core.DTOs.Project.Response;
public record ProjectDetailsResponseDto(
    Guid Id,
    string Title,
    string Description,
    string CategoryName,
    int ProgressPercentage,
    string TimeLeftText,
    List<string> Skills,
    List<string> ProjectGoals,
    DateTime StartDate,
    DateTime Deadline,
    string? CoverImageUrl,
    string Status
);
