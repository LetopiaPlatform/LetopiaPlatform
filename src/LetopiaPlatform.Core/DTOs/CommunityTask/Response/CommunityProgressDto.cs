namespace LetopiaPlatform.Core.DTOs.CommunityTask.Response;
public record CommunityProgressDto(
    int TotalTasks,
    int CompletedTasks,
    double ProgressPercentage,
    string ProgressMessage
);
