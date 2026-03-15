
using LetopiaPlatform.Core.Entities;

namespace LetopiaPlatform.Core.Enums;

/// <summary>
/// Identifies which entity type a <see cref="Tag"/> belongs to.
/// Adding a new taggable entity only requires a new enum value here —
/// no schema or entity changes needed.
/// </summary>
public enum TagTarget
{
    Post = 1,
    Resource = 2,
}
