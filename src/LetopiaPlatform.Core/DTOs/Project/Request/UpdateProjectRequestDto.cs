using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.DTOs.Project.Request;
public record UpdateProjectRequestDto(
    string Title,
    string Description,
    Guid CategoryId,
    string? DifficultyLevel,
    List<string> RequiredSkills,
    List<string>? Links,
    List<IFormFile>? Files
);
