using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.DTOs.Project.Request;
public record UpdateProjectRequestDto(
    Guid Id,
    string Title,
    string Description,
    Guid CategoryId,
    string? DifficultyLevel,
    DateTime StartDate,
    DateTime EndDate,
    int MaxMembers,
    List<string> RequiredSkills,
    List<string> Goals,
    IFormFile? CoverImage // اختياري في حالة التحديث
);
