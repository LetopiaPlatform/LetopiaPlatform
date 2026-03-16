namespace LetopiaPlatform.Core.DTOs.CommunityResourse;

/// <param name="Url">The canonical URL of the previewed page.</param>
/// <param name="Title">og:title scraped from the page, if present.</param>
/// <param name="Description">og:description scraped from the page, if present.</param>
/// <param name="Image">og:image scraped from the page, if present.</param>
public record LinkPreviewDto(
    string Url,
    string? Title = null,
    string? Description = null,
    string? Image = null);
