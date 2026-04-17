namespace LetopiaPlatform.Core.DTOs.CommunityTaskCategory.Response;
public record TaskCategoryResponseDto(
    Guid Id,
    string Name,
    string ColorHex,
    string? IconKey,
    int TotalTasksCount,
    int CompletedTasksCount,
    double ProgressPercentage
);
