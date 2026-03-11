using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.CommunityResourse;
public record ResourceDto
{
    public Guid Id { get; init; }

    public string Title { get; init; } = default!;

    public string Url { get; init; } = default!;

    public string? ThumbnailUrl { get; init; }

    public string? Description { get; init; }

    public ResourceType Type { get; init; }

    public int ViewsCount { get; init; }

    public int LikesCount { get; init; }

    public bool IsLikedByCurrentUser { get; init; }

    public List<string> Tags { get; init; } = new();

    /// <summary>Brief info about who uploaded this resource.</summary>
    public UploadedByDto UploadedBy { get; init; } = default!;

    public DateTime CreatedAt { get; init; }
}
