namespace LetopiaPlatform.Core.DTOs.ProjectCategory.Response;
public record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? IconUrl,
    int DisplayOrder,
    List<ProjectSummaryResponse> Projects
);

public record ProjectSummaryResponse(
    Guid Id,
    string Title
);
