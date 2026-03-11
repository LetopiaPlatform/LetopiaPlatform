using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LetopiaPlatform.Core.DTOs.CommunityResourse;
public record LinkPreviewDto
{
    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? Image { get; init; }

    public string Url { get; init; } = default!;
}
