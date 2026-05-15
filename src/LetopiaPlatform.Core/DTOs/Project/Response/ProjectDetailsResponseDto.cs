namespace LetopiaPlatform.Core.DTOs.Project.Response;

public record ProjectDetailsResponseDto(
    Guid Id,
    string Title,
    string Description,
    string CategoryName,
    List<string> Skills,
    string Status,
    string OwnerName,
    string OwnerPictureUrl,
    int Progress,
    List<MilestoneResponseDto> Milestones,
    List<ResourceResponseDto> Resources
);

public record MilestoneResponseDto(
    Guid Id,
    string Title,
    string? Description,
    string? DurationText,
    string Status,
    int CalculatedProgress
);

public record ResourceResponseDto(
    Guid Id,
    string Name,
    string Url,
    bool IsFile
);
