using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.CommunityResourse;

/// <param name="Url">The resource URL to save and scrape.</param>
/// <param name="Type">The resource type (e.g. Article, Video).</param>
/// <param name="Title">Optional — falls back to og:title scraped from the URL.</param>
/// <param name="Description">Optional — falls back to og:description scraped from the URL.</param>
/// <param name="Tags">Optional tags e.g. ["ASP.NET", "Docker"].</param>
public record CreateResourceRequest(
    string Url,
    ResourceType Type,
    string? Title = null,
    string? Description = null,
    List<string>? Tags = null);
