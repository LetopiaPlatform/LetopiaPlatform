using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Project.Request;
public record MilestoneRequestDto(
    string Title,
    string? Description,
    string? DurationText,
    MilestoneStatus Status
);
