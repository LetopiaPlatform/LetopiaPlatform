
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Entities;
/// <summary>
/// A generic tag that can be attached to any taggable entity via
/// <see cref="TargetType"/> + <see cref="TargetId"/>.
/// A unique index on (TargetType, TargetId, TagName) prevents duplicates.
/// </summary>
public class Tag
{
    public Guid Id { get; set; }

    /// <summary>Which entity type this tag belongs to (Post, Resource, …).</summary>
    public TagTarget TargetType { get; set; }

    /// <summary>The PK of the tagged entity (PostId, ResourceId, …).</summary>
    public Guid TargetId { get; set; }

    /// <summary>Lowercase normalized tag value e.g. "dotnet", "docker".</summary>
    public string TagName { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}
