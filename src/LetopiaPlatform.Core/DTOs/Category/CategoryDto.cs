namespace LetopiaPlatform.Core.DTOs.Category;

/// <summary>
/// Read-only category representation for API responses.
/// </summary>
public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? IconUrl,
    string Type
);
