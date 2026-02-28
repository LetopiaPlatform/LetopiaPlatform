using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.DTOs.Project.Request;
public record CreateProjectRequestDto(
   string Title,
    string Description,
    Guid CategoryId,
    Guid OwnerId,
    string? DifficultyLevel,
    DateTime StartDate,
    DateTime EndDate,
    int MaxMembers,
    List<string> RequiredSkills,
    List<string> Goals,
    IFormFile? CoverImage
);

