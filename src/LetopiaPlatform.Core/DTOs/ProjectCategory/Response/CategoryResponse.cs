namespace LetopiaPlatform.Core.DTOs.ProjectCategory.Response;
public record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? IconUrl,
    int DisplayOrder,
    List<ProjectSummaryResponse> Projects // القائمة اللي ضفناها في الـ Mapping
);

public record ProjectSummaryResponse(
    Guid Id,
    string Title
);
