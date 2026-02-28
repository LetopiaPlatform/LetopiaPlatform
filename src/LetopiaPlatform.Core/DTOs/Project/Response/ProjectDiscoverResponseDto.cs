namespace LetopiaPlatform.Core.DTOs.Project.Response;
public record ProjectDiscoverResponseDto(
    Guid Id,
    string Title,
    string CategoryName,
    string? DifficultyLevel,
    string Status,
    List<string> Skills,
    string? CoverImageUrl
);
