using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.CommunityResourse;
public record CreateResourceRequest
{


    public string Url { get; init; } = default!;

    public ResourceType Type { get; init; }

    /// <summary>Optional — falls back to og:title scraped from the URL.</summary>
    public string? Title { get; init; }

    /// <summary>Optional — falls back to og:description scraped from the URL.</summary>
    public string? Description { get; init; }

    /// <summary>Optional tags e.g. ["ASP.NET", "Docker"].</summary>
    public List<string> Tags { get; init; } = new();
}
