namespace LetopiaPlatform.Core.DTOs.Project.Response;
public record ProjectDiscoverResponseDto(
    Guid Id,
    string Title,
    string CategoryName,
    string? DifficultyLevel, // Beginner, Intermediate, Advanced
    string Status,           // Recruiting, In Progress, etc.
    List<string> Skills,     // Displayed as Chips in UI
    int MembersCount,        // Number of current team members
    Guid OwnerId,
    string PictureUrl,                         // To allow navigation to Owner's profile
    string OwnerName         // To display "By: OwnerName" in the card
);
