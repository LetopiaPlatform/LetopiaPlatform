using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.DTOs.Project.Request;
public record CreateProjectRequestDto(
     string Title,
     string Description,
     Guid CategoryId,

    // (Public / Private)
    bool IsPublic,

    string? DifficultyLevel,

    DateTime StartDate,
    DateTime EndDate,


    List<string> RequiredSkills,
    List<string> Goals,

    List<string> TimelineEvents,

    List<CreateMilestoneDto> Milestones,


    IFormFile? CoverImage
);



