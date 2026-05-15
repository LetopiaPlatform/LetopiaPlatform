using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.DTOs.Project.Request;

public record CreateProjectRequestDto(
    string Title,
    string Description,
    Guid CategoryId,

    // (Public / Private)
    bool IsPublic,

    string? DifficultyLevel,

    List<string> RequiredSkills,

    List<string>? Links,

    List<IFormFile>? Files

);
