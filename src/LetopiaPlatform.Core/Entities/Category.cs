using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Entities;

/// <summary>
/// Shared category entity used across domains (Community, Project).
/// Admin-managed with icon, and unique slug per type.
/// </summary>
public class Category : BaseEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? IconUrl { get; set; }
    public CategoryType Type { get; set; }
}