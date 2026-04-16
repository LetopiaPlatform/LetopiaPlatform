namespace LetopiaPlatform.Core.DTOs.Search;

public sealed record ProjectSearchResultDto(
    Guid Id,
    string Title,
    string Description,
    string? CoverImageUrl,
    string CategoryName,
    string? DifficultyLevel,
    string Status);