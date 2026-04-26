namespace LetopiaPlatform.Core.DTOs.Project.Response;
public record ProjectDiscoverResponseDto(
    Guid Id,
    string Title,
    string CategoryName,
    string? DifficultyLevel, // Beginner, Intermediate, Advanced
    string Status,           // Recruiting, In Progress, etc.
    List<string> Skills,     // Displayed as Chips in UI
    string? CoverImageUrl,
    int MembersCount,        // Number of current team members
    string TimeLeft,         // Calculated string (e.g., "3 days left")
    Guid OwnerId,            // To allow navigation to Owner's profile
    string OwnerName         // To display "By: OwnerName" in the card
);
