using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.CommunityResourse;
public record ResourceQueryParams
{
    public ResourceType? Type { get; init; }

    public string? Tag { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
